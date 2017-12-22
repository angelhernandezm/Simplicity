namespace Simplicity.dotNet.Daemon {
	partial class ProjectInstaller {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.SimplicityDaemonInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.SimplicityDaemonServiceInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// SimplicityDaemonInstaller
			// 
			this.SimplicityDaemonInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.SimplicityDaemonInstaller.Password = null;
			this.SimplicityDaemonInstaller.Username = null;
			// 
			// SimplicityDaemonServiceInstaller
			// 
			this.SimplicityDaemonServiceInstaller.Description = "Simplicity Daemon responsible for processing Jar files, generating .NET assemblie" +
    "s (acting as proxies) and dynamically hosting them over HTTP.";
			this.SimplicityDaemonServiceInstaller.DisplayName = "Simplicity Daemon";
			this.SimplicityDaemonServiceInstaller.ServiceName = "SimplicityDaemon";
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.SimplicityDaemonInstaller,
            this.SimplicityDaemonServiceInstaller});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller SimplicityDaemonInstaller;
		private System.ServiceProcess.ServiceInstaller SimplicityDaemonServiceInstaller;
	}
}