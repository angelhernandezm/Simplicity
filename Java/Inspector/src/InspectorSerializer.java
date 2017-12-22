package com.simplicity;

import java.io.File;
import java.util.Map;
import java.util.List;

import org.w3c.dom.Attr;

import java.io.IOException;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import com.simplicity.Common;

import java.lang.reflect.Method;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

/**
 *
 */
public class InspectorSerializer {
    /**
     *
     */
    class ClassReferenceCount {
        int count = 0;

        int increaseByOne() {
            return ++count;
        }
    }

    /**
     * @param classesAndMethods
     * @param xmlFilePath
     * @return
     */
    public boolean serializeReflectedMethods(Map<String, List<Method>> classesAndMethods, String xmlFilePath)
            throws IOException {
        File xmlFile;
        boolean retval = false;
        final ClassReferenceCount classCount = new ClassReferenceCount();

        if (classesAndMethods != null && classesAndMethods.size() > 0 && !Common.IsNullOrEmpty(xmlFilePath)) {
            try {
                // We'll remove file if it already exists
                if ((xmlFile = new File(xmlFilePath)).exists())
                    xmlFile.delete();

                DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
                DocumentBuilder docBuilder = docFactory.newDocumentBuilder();

                // root element
                Document doc = docBuilder.newDocument();
                Element rootElement = doc.createElement("simplicity");
                doc.appendChild(rootElement);

                // class elements
                classesAndMethods.forEach((k, v) -> {
                    Element classNode = doc.createElement("class");
                    rootElement.appendChild(classNode);
                    doc.createAttribute("name");
                    doc.createAttribute("index");
                    doc.createAttribute("methodCount");
                    classNode.setAttribute("name", k);
                    classNode.setAttribute("index", String.valueOf(classCount.increaseByOne()));
                    classNode.setAttribute("methodCount", String.valueOf(v.size()));
                    writeElements(classNode, v);
                });

                // Let's serialize XML Document
                TransformerFactory transformerFactory = TransformerFactory.newInstance();
                Transformer transformer = transformerFactory.newTransformer();
                DOMSource source = new DOMSource(doc);
                StreamResult serializedFile = new StreamResult(new File(xmlFilePath));
                transformer.transform(source, serializedFile);
                retval = true;
            } catch (Exception ex) {
                System.out.print(ex.getMessage());
            }
        }

        return retval;
    }

    /***
     *
     * @param parent
     * @param methods
     */
    private void writeElements(Element parent, List<Method> methods) {
        if (parent != null && methods != null && methods.size() > 0) {
            Document doc =  parent.getOwnerDocument();
            for (Method m : methods) {
                Element methodNode = doc.createElement("Method");
                methodNode.appendChild(doc.createTextNode(m.toString()));
                parent.appendChild(methodNode);
            }
        }
    }


}