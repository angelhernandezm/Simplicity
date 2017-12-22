package com.simplicity;

/**
 *
 */
public class Common {
    /**
     *
     * @param testString
     * @return
     */
    public static boolean IsNullOrEmpty(String testString) {
        return (testString == null || testString.trim().equals(""));
    }
}
