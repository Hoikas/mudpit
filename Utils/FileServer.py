#!/usr/bin/env python
import os
import os.path
import subprocess
import sys

input = "G:\\Plasma\\Games\\MUd MOUL"
output = "G:\\Plasma\\Servers\\Manifest Data"

def do_help():
    print "\tWelcome to the MUd Manifest Generator!"
    print
    print "\t--input=<directory>"
    print "\tThe Uru Install we will build from"
    print "\tDefaults to " + input
    print
    print "\t--output=<directory>"
    print "\tWhere the finished data server files are stored"
    print "\tDefaults to " + output


if __name__ == "__main__":
    if "--help" in sys.argv:
        doHelp()
    else:
        for arg in sys.argv:
            pair = arg.split("=")
            if pair[0] == "--input":
                input = pair[1]
            elif pair[0] == "--output":
                output = pair[1]
    
    ###EXTERNAL
    subprocess.call(["python", "MfsGen.py", "--input=" + input, "--output=" + output, "--age=EXTERNAL"])
    
    ###AGES
    dat_in = os.path.join(input, "dat")
    list = os.listdir(dat_in)
    for cosa in list:
        if cosa.find(".age") != -1:
            cosa = cosa.replace(".age", "")
            subprocess.call(["python", "MfsGen.py", "--input=" + input, "--output=" + output, "--age=" + cosa])