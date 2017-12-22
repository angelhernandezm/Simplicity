using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient {
	public partial class frmMain : Form {
		/// <summary>
		/// The sb output manual
		/// </summary>
		private StringBuilder _sbOutputManual = new StringBuilder();

		/// <summary>
		/// The sb output random
		/// </summary>
		private StringBuilder _sbOutputRandom = new StringBuilder();

		/// <summary>
		/// Initializes a new instance of the <see cref="frmMain"/> class.
		/// </summary>
		public frmMain() {
			InitializeComponent();
			cboOperation.SelectedIndex = 0;
		}

		/// <summary>
		/// Handles the Click event of the btnManualCalculate control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void btnManualCalculate_Click(object sender, EventArgs e) {
			if (!string.IsNullOrEmpty(txtOperand1.Text) && !string.IsNullOrEmpty(txtOperand2.Text)) {
				try {
					var operators = new string[] { "+", "-", "x", "÷" };
					var methods = new List<Func<float, float, float>>();

					using (var proxy = new Simplicity.Services.CalculatorClient()) {
						methods.Add(proxy.Sum);
						methods.Add(proxy.Subtract);
						methods.Add(proxy.Multiply);
						methods.Add(proxy.Divide);
						var result = methods[cboOperation.SelectedIndex](float.Parse(txtOperand1.Text), float.Parse(txtOperand2.Text));
						_sbOutputManual.AppendLine($"{txtOperand1.Text} {operators[cboOperation.SelectedIndex]} {txtOperand2.Text} = {result}");
						txtOutput.Text = _sbOutputManual.ToString();
					}
				} catch (Exception ex) {
					MessageBox.Show(ex.ToString());
				}
			} else
				MessageBox.Show("Please enter operands and select operation to perform.");

		}

		/// <summary>
		/// Handles the Click event of the btnRandomCalculate control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void btnRandomCalculate_Click(object sender, EventArgs e) {
			try {
				var random = new Random();
				var randomOp = new Random();
				var operators = new string[] { "+", "-", "x", "÷" };
				var methods = new List<Func<float, float, float>>();
				var values = new List<KeyValuePair<float, float>>();
				var operationCount = int.Parse(txtOperationCount.Text);
				Enumerable.Range(1, operationCount).Select(p => p * p).ToList().ForEach(z => {
					values.Add(new KeyValuePair<float, float>(random.Next(1, 150),
							   random.Next(150, 300)));
				});

				using (var proxy = new Simplicity.Services.CalculatorClient()) {
					methods.Add(proxy.Sum);
					methods.Add(proxy.Subtract);
					methods.Add(proxy.Multiply);
					methods.Add(proxy.Divide);

					if (!chkRunInParallel.Checked) {
						values.ForEach(r => {
							var selected = randomOp.Next(0, 3);
							var result = methods[selected](r.Key, r.Value);

							new Thread(() => {
								_sbOutputRandom.AppendLine($"{r.Key} {operators[selected]} {r.Value} = {result}");
								txtOutputRandom.Invoke(new MethodInvoker(delegate {
									txtOutputRandom.Text = _sbOutputRandom.ToString();
								}));
							}).Start();
						});
					} else {
						Parallel.ForEach(values, (r) => {
							var selected = randomOp.Next(0, 3);
							var result = methods[selected](r.Key, r.Value);

							new Thread(() => {
								_sbOutputRandom.AppendLine($"{r.Key} {operators[selected]} {r.Value} = {result}");
								txtOutputRandom.Invoke(new MethodInvoker(delegate {
									txtOutputRandom.Text = _sbOutputRandom.ToString();
								}));
							}).Start();
						});
					}
				}
			} catch (Exception ex) {
				System.Diagnostics.Trace.WriteLine(ex.ToString());
			}


		}
	}
}
