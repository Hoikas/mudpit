#!/usr/bin/env python
from PyPlasma import *
import gzip
import hashlib
import os
import os.path
import shutil
import sys

age_name = "EXTERNAL"
input = "G:\\Plasma\\Games\\MUd MOUL"
output = "G:\\Plasma\\Servers\\Manifest Data"

EXCLUDE = []

kFlagStereoOgg = 0x1
kFlagUnkAlwaysExternal = 0x2
kFlagDecompressOgg = 0x4

def do_help():
    print "\tWelcome to the MUd Manifest Generator!"
    print
    print "\t--age=<age_name>"
    print "\tThe age you would like to build a manifest for."
    print "\tLeaving this out or setting to \"external\" will generate the patcher manifests"
    print
    print "\t--input=<directory>"
    print "\tThe Uru Install we will build from"
    print "\tDefaults to " + input
    print
    print "\t--output=<directory>"
    print "\tWhere the finished data server files are stored"
    print "\tDefaults to " + output


def process_file(file, out_dir, flag="0"):
    if file in EXCLUDE:
        pass
    
    full_out = os.path.join(output, out_dir)
    down_path = os.path.join(out_dir, file + ".gz") #Download Name
    if not os.path.exists(full_out):
        os.mkdir(full_out)
    full_out = os.path.join(full_out, file + ".gz") #Path to the output file
    
    full_in = os.path.join(input, file) #The path to the input file
    
    #Check 1-2-3
    if not os.path.isfile(full_in):
        print "%s does not exist!" % file
        return None
    
    un_size = os.path.getsize(full_in) #Uncompressed filesize
    
    #Hash it
    handle = open(full_in, "rb")
    hash = hashlib.md5(handle.read(un_size)).hexdigest()
    handle.close()
    
    #Compress it
    handle = open(full_in, "rb")
    gz = gzip.open(full_out, "wb")
    gz.writelines(handle)
    gz.close()
    handle.close()
    
    #Final stuff
    c_size = os.path.getsize(full_out)
    
    #Hash again...
    handle = open(full_out, "rb")
    c_hash = hashlib.md5(handle.read(c_size)).hexdigest()
    handle.close()
    
    line = "%s,%s,%s,%s,%s,%s,%s" % (file, down_path, hash, c_hash, str(un_size), str(c_size), str(flag))
    
    print line
    return line + "\n"    


def process_directory(dir, out_dir, constraints = [], restraints = []):
    full_dir = os.path.join(input, dir)
    base = os.listdir(full_dir)
    contents = ""
    for file in base:
        if os.path.isfile(os.path.join(full_dir, file)):
            if len(constraints) != 0:
                yay = False
                for c in constraints:
                    if file.find(c) != -1:
                        yay = True
                        break
                
                if not yay:
                    continue
            
            if len(restraints) != 0:
                boo = False
                for r in restraints:
                    if file.find(r) != -1:
                        boo = True
                        break
                
                if boo:
                    continue
            
            contents += process_file(os.path.join(dir, file), out_dir)
    
    return contents

def make_age_mfs(age, name):
    age.write("###Pages\n")
    age.write(process_directory("dat", "Client", [name + "_District"], [".age", ".fni", ".sum", ".csv", ".p2f", ".loc"]))
    
    age.write("\n\n###Sounds\n")
    sounds = {}
    
    #This is going to get a bit hairy...
    mgr = plResManager()
    file = os.path.join(input, "dat", name + ".age")
    print file
    mgr.ReadAge(file, True)
    locs = mgr.getLocations()
    done = []
    for loc in locs:
        keys = mgr.getKeys(loc, plSoundBuffer().ClassIndex())
        for key in keys:
            sound = plSoundBuffer.Convert(key.object)
            if sound.fileName in done:
                continue
            else:
                done.append(sound.fileName)
            
            flags = 0x00
            if not (sound.flags & plSoundBuffer.kStreamCompressed):
                flags |= kFlagDecompressOgg
            else:
                if (sound.flags & plSoundBuffer.kOnlyRightChannel) or (sound.flags & plSoundBuffer.kOnlyLeftChannel):
                    flags |= kFlagStereoOgg
                
                if (sound.flags & plSoundBuffer.kAlwaysExternal):
                    flags |= kFlagUnkAlwaysExternal
            
            line = process_file(os.path.join("sfx", sound.fileName), "Client", str(flags))
            if line != None:
                age.write(line)


def make_external_mfs(external):
    external.write("###Main Client\n")
    external.write(process_directory("", "Client", [], ["UruLauncher"]))
    
    external.write("\n\n###Avi Directory\n")
    external.write(process_directory("avi", "Client", [".bik"]))
    
    external.write("\n\n###Data Directory\n")
    external.write(process_directory("dat", "Client", [".p2f", ".age", ".csv", ".fni", ".loc"], ["UruLauncher"]))


def make_thin_external_mfs(thin):
    external.write(process_file("UruExplorer.exe", "Client"))
    external.write(process_file("msvcp71.dll", "Client"))
    external.write(process_file("msvcr71.dll", "Client"))
    external.write(process_file("NxCooking.dll", "Client"))
    external.write(process_file("NxCharacter.dll", "Client"))
    external.write(process_file("NxExtensions.dll", "Client"))
    external.write(process_file("OpenAL32.dll", "Client"))
    external.write(process_file("ReleaseNotes.txt", "Client"))
    external.write(process_file("wrap_oal.dll", "Client"))


def make_patcher_mfs(patcher):
    patcher.write(process_file("UruLauncher.exe", "Patcher"))



if __name__ == "__main__":
    if "--help" in sys.argv:
        do_help()
    else:
        for arg in sys.argv:
            pair = arg.split("=")
            if pair[0] == "--age":
                age_name = pair[1]
            elif pair[0] == "--input":
                input = pair[1]
            elif pair[0] == "--output":
                output = pair[1]
    
    if age_name.upper() == "EXTERNAL":
        patcher = open(os.path.join(output, "ExternalPatcher.mfs"), "w")
        make_patcher_mfs(patcher)
        patcher.close()
        
        external = open(os.path.join(output, "External.mfs"), "w")
        make_external_mfs(external)
        external.close()
        
        external = open(os.path.join(output, "ThinExternal.mfs"), "w")
        make_thin_external_mfs(external)
        external.close()
    else:
        age = open(os.path.join(output, age_name + ".mfs"), "w")
        make_age_mfs(age, age_name)
        age.close()