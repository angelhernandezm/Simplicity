package com.simplicity;

import com.simplicity.Common;

import java.io.*;
import java.lang.reflect.Method;
import java.net.URL;
import java.net.URLClassLoader;
import java.util.*;
import java.util.jar.JarEntry;
import java.util.jar.JarFile;
import java.util.jar.JarInputStream;

/**
 *
 */
public class Inspector {
    /**
     *
     * @return
     */
    public static Inspector Create() {
        return new Inspector();
    }

    /**
     *
     * @param targetJar
     * @return
     * @throws ClassNotFoundException
     * @throws IOException
     */
    public Map<String, List<Method>> getMethodsInClasses(String targetJar) throws ClassNotFoundException, IOException {
        JarFile jarFile = new JarFile(targetJar);
        Enumeration<JarEntry> e = jarFile.entries();
        URL[] urls = {new URL("jar:file:" + targetJar + "!/")};
        URLClassLoader cl = URLClassLoader.newInstance(urls);
        Map<String, List<Method>> retval = new HashMap<String, List<Method>>();

        while (e.hasMoreElements()) {
            JarEntry je = e.nextElement();
            if (je.isDirectory() || !je.getName().endsWith(".class")) {
                continue;
            }
            // -6 because of .class
            String clsName = je.getName().substring(0, je.getName().length() - 6);
            clsName = clsName.toString().replace('/', '.');
            Class c = cl.loadClass(clsName);
            retval.put(clsName, Arrays.asList(c.getMethods()));
        }

        return retval;
    }

    /***
     *
     * @param ex
     * @return
     */
    public static String extractExceptionDetails(Object ex) {
        String retval = "";

        try {
            StringWriter sw = new StringWriter();
            PrintWriter pw = new PrintWriter(sw);
            Exception jniEx =  new Exception((Throwable) ex);
            jniEx.printStackTrace(pw);
            String stackTrace =  sw.toString();
            retval = String.format("Error: %s\nStackTrace: %s", jniEx.getMessage(), stackTrace);
            pw.close();
            sw.close();
        } catch(Exception e) {
            retval = "Unable to cast from JNI pointer - " + e.getCause().toString();
        }
        return retval;
    }

    /**
     *
     * @param targetJar
     * @return
     * @throws IOException
     * @throws IllegalArgumentException
     */
    public List<String> getClassesInJar(String targetJar) throws IOException, IllegalArgumentException {
        JarEntry jarEntry;
        List<String> retval = null;
        JarInputStream stream = null;

        if (!Common.IsNullOrEmpty(targetJar) && (new File(targetJar)).exists()) {
            try {
                retval = new ArrayList<String>();
                stream = new JarInputStream(new FileInputStream(targetJar));

                while ((jarEntry = stream.getNextJarEntry()) != null) {
                    if (jarEntry.getName().endsWith(".class")) {
                        String className = jarEntry.getName().replaceAll("/", "\\.");
                        String newClassName = className.substring(0, className.lastIndexOf('.'));
                        retval.add(newClassName);
                    }
                }
            } catch (Exception ex) {
                retval = null;
            } finally {
                stream.close();
            }
        }

        return retval;
    }

    /**
     *
     * @param s
     * @return
     * @throws Exception
     */
     public static String addPath(String s) throws Exception {
        int before, after;
        File f = new File(s);
        URL u = f.toURL();
        URLClassLoader urlClassLoader = (URLClassLoader) ClassLoader.getSystemClassLoader();
        Class urlClass = URLClassLoader.class;
        Method method = urlClass.getDeclaredMethod("addURL", new Class[]{URL.class});
        method.setAccessible(true);
        before = urlClassLoader.getURLs().length;
        method.invoke(urlClassLoader, new Object[]{u});
        after = urlClassLoader.getURLs().length;
        return String.format("before:%d - after:%d", before, after);
    }
}