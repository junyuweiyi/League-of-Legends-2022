
import os
import sys

def remove(inFolder, keyword):
    for parent, _dirNames, fileNames in os.walk(inFolder):
        for fileName in fileNames:
            filePath = os.path.join(parent, fileName)
            lines = []
            with open(filePath, 'r', newline='', encoding='utf-8') as file:
                for line in file.readlines():
                    if not keyword in line:
                        lines.append(line)
            
            with open(filePath, 'w', newline='', encoding='utf-8') as file:
                file.writelines(lines)
            

def main():
    if len(sys.argv) < 3:
        print('remove_lines.py error arguments not enough')
        return 1
    remove(sys.argv[1], sys.argv[2])

if __name__ == "__main__":
    main()