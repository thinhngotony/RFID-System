// +build !linux

package Common

import (
	"io"
	"log"
	"os"
)

var singleInstanceFile *os.File

func CheckSingleInstance(lockFile string) {

	if singleInstanceFile != nil {
		singleInstanceFile.Close()
	}

	if _, err := os.Stat(lockFile); err == nil {
		err := os.Remove(lockFile)
		if err != nil {
			log.Fatalln("Instance existed, stop process!")
		}
	}

	file, err := os.OpenFile(lockFile, os.O_CREATE|os.O_WRONLY, 0666)

	if err == nil {
		singleInstanceFile = file
	}
}

func GetLogFile() io.Writer {
	return os.Stdout
}
