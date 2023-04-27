// +build linux

package Common

import (
	"github.com/goframework/gf/exterror"
	"io"
	"log"
	"os"
	"path/filepath"
	"syscall"
)

var singleInstanceFile *os.File

func CheckSingleInstance(lockFile string) {

	if singleInstanceFile != nil {
		singleInstanceFile.Close()
	}

	var file *os.File
	var err error
	if _, err := os.Stat(lockFile); os.IsNotExist(err) {
		file, err = os.Create(lockFile)
	} else {
		file, err = os.OpenFile(lockFile, os.O_WRONLY, 0666)
	}

	if err == nil {
		err = syscall.Flock(int(file.Fd()), syscall.LOCK_EX|syscall.LOCK_NB)
	}

	if err == nil {
		singleInstanceFile = file
	} else {
		log.Fatalln("Instance existed, stop process!")
	}
}

func GetLogFile() io.Writer {
	dir, _ := filepath.Abs(filepath.Dir(os.Args[0]))
	dir = filepath.Join(dir, "log")
	os.MkdirAll(dir, os.ModePerm)

	logFile := filepath.Join(dir, CurrentDate()+".log")

	f, err := os.OpenFile(logFile, os.O_CREATE|os.O_APPEND|os.O_RDWR, os.ModePerm)

	if err != nil {
		log.Println(exterror.WrapExtError(err))
	}

	return f
}
