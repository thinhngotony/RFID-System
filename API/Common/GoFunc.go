package Common

import (
	"log"
	"runtime/debug"
)

type SafeGo struct {
	Exec func()
}

func Recover() {
	if rec := recover(); rec != nil {
		log.Printf("[RECOVER]: %v\r\n\t%s\r\n", rec, debug.Stack())
	}
}

func (sg SafeGo) Go() {
	go func() {
		defer Recover()
		sg.Exec()
	}()
}
