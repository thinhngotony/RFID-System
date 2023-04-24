package Common

import (
	"log"
	"os"
	"path/filepath"
	"time"

	"github.com/goframework/gf/ext"
	"github.com/goframework/gf/exterror"
)

const (
	FAIL_CAUSE_BQ_CONNECT = "BQ接続に失敗しました。"
	FAIL_CAUSE_BQ_QUERY   = "BQクエリに失敗しました。"
	FAIL_CAUSE_BQ_IMPORT  = "BQインポートに失敗しました。"

	FAIL_CAUSE_DB_CONNECT      = "DB接続に失敗しました。"
	FAIL_CAUSE_DB_QUERY        = "DBクエリに失敗しました。"
	FAIL_CAUSE_DB_TX_BEGIN     = "DBトランザション開始に失敗しました。"
	FAIL_CAUSE_DB_TX_END       = "DBトランザション終了に失敗しました。"
	FAIL_CAUSE_DB_DATA_GET     = "DBデータ取得に失敗しました。"
	FAIL_CAUSE_DB_DATA_INSERT  = "DBデータ追加に失敗しました。"
	FAIL_CAUSE_DB_DATA_UPDATE  = "DBデータ更新に失敗しました。"
	FAIL_CAUSE_TSV_FILE_CREATE = "TSVファイル作成に失敗しました。"
	FAIL_CAUSE_CSV_FILE_CREATE = "CSVファイル作成に失敗しました。"
	FAIL_CAUSE_TSV_FILE_WRITE  = "TSVデータ書入に失敗しました。"
	FAIL_CAUSE_CSV_FILE_WRITE  = "CSVデータ書入に失敗しました。"
	FAIL_CAUSE_GCS_CONNECT     = "GCS接続に失敗しました。"
	FAIL_CAUSE_GCS_UPLOAD      = "GCSファイルアップロードに失敗しました。"

	FAIL_CAUSE_FTP_CONNECT = "FTP接続に失敗しました。"
	FAIL_CAUSE_FTP_LOGIN   = "FTPログインに失敗しました。"
	FAIL_CAUSE_FTP_UPLOAD  = "FTPファイルアップロードに失敗しました。"

	ERR_QUERY_BUILD = "Error on query build"

	LOG_SPLITTER = "--------------------------------------------------"
)

type ErrorDetail struct {
	Err    error
	Detail string
}

func init() {
	execName, e := os.Executable()

	if e == nil {
		logDir := execName + ".logd"
		if !ext.FolderExists(logDir) {
			os.MkdirAll(logDir, os.ModePerm)
		}

		SafeGo{Exec: func() {
			var lf *os.File
			var lastDate = ""
			for {
				currentDate := CurrentDate()
				if currentDate != lastDate {
					lastDate = currentDate
					if lf != nil {
						lf.Close()
						lf = nil
					}
				}

				logFilePath := filepath.Join(logDir, lastDate+".log")
				lf, e = os.OpenFile(logFilePath, os.O_APPEND|os.O_CREATE|os.O_WRONLY, os.ModePerm)
				if e == nil {
					log.SetOutput(lf)
				}
				time.Sleep(time.Minute)
			}
		}}.Go()
	}
}

func PrintLog(v ...interface{}) {
	log.Println(v...)
}

func LogError(err *ErrorDetail) {
	if err != nil && err.Err != nil {
		log.Printf("共通エラー: %v\n\t%v", err.Detail, err.Err)
	}
}

func LogJobError(id int64, err *ErrorDetail) {
	if err != nil && err.Err != nil {
		log.Printf("ジョブエラー: ID「%d」\n\t%v\n\t%v", id, err.Detail, err.Err)
	}
}

func InsertToJobLogTable(pgName string, pgNameJP string, pgStepId string, pgResult string, errOk string) error {
	PrintLog("InsertToJobLogTable begin")
	defer PrintLog("InsertToJobLogTable end")

	conn, err := GetDBUtil()
	if err != nil {
		return exterror.WrapExtError(err)
	}
	defer conn.Close()

	query := `INSERT INTO drfid_joblog values ( ?, ?, ?, ?, ?, ?)`

	//insert to database
	_, err = conn.DB.Exec(query, time.Now(), pgName, pgNameJP, pgStepId, pgResult, errOk)

	if err != nil {
		return exterror.WrapExtError(err)
	}

	return nil
}
