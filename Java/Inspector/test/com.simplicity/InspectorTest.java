package com.simplicity;

import org.junit.Assert;
import org.junit.Test;
import java.io.IOException;
import java.lang.reflect.Method;
import java.net.URL;
import java.net.URLClassLoader;
import java.nio.file.Paths;
import java.util.List;
import java.util.Map;

/**
 *
 */
public class InspectorTest {
    final String TestJarFile = "../../TestJar/SimpleCalc.jar";
    final String TestOutputXmlFile = "c:\\Temp\\OutPutFile.xml";

    /**
     *
     * @return
     */
    private static String GetTestJarPath() {
        String retval =  Paths.get(".").toAbsolutePath().toString().replace("Inspector", "");
        retval = retval.substring(0, retval.length() - 2);
        retval += "TestJar\\SimpleCalc.jar";

        return retval;
    }

    /**
     *
     * @throws IOException
     * @throws ClassNotFoundException
     */
    @Test
    public void ExtractMethodsFromJar() throws IOException, ClassNotFoundException {
        String jarPath = GetTestJarPath();
        com.simplicity.Inspector instance = com.simplicity.Inspector.Create();
        Map<String, List<Method>> methods = instance.getMethodsInClasses(jarPath);
        Assert.assertTrue(methods != null);
        Assert.assertTrue(methods.size() > 0);
    }

    /**
     *
     * @throws IOException
     */
    @Test
    public void ExtractClassesFromJar() throws IOException {
        String jarPath = GetTestJarPath();
        com.simplicity.Inspector instance = com.simplicity.Inspector.Create();
        List<String> classes = instance.getClassesInJar(jarPath);
        Assert.assertTrue(classes != null);
        Assert.assertTrue(classes.size() > 0);
    }

    /**
     *
     * @throws IOException
     * @throws ClassNotFoundException
     */
    @Test
    public void GenerateMethodXmlFile() throws IOException, ClassNotFoundException {
        String jarPath = GetTestJarPath();
        com.simplicity.Inspector instance = com.simplicity.Inspector.Create();
        com.simplicity.InspectorSerializer serializer = new com.simplicity.InspectorSerializer();
        boolean result = serializer.serializeReflectedMethods(instance.getMethodsInClasses(jarPath), TestOutputXmlFile);
        Assert.assertTrue(result);
    }

    /**
     *
     * @throws Exception
     */
    @Test
    public void AddPath() throws Exception {
        String result;
        String jarPath = GetTestJarPath();
        ClassLoader cl = ClassLoader.getSystemClassLoader();
        URL[] urlsBefore = ((URLClassLoader)cl).getURLs();
        result = com.simplicity.Inspector.addPath(jarPath);
        URL[] urlsAfter = ((URLClassLoader)cl).getURLs();
        Assert.assertTrue(urlsAfter.length > urlsBefore.length);
    }
}