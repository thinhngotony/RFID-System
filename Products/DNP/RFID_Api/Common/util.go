package Common

import (
	"fmt"
	"log"
	"main/db"
	"main/gcp"
	"main/gcp/bq"
	"main/gcp/st"
	"main/mailsender"
	"math/rand"
	"net/mail"
	"os"
	"path"
	"path/filepath"
	"strings"
	"time"

	bqc "ProductManage/github.com/goframework/gcp/bq"

	"ProductManage/github.com/goftp/ftp"

	"github.com/goframework/gf/exterror"
)

func Rand32bitStr() string {
	return fmt.Sprintf("%x", rand.Uint32())
}

const (
	MaxRetry            = 3
	DNPGenreCd          = "04"
	LOG_RESULT_NORMAL   = "正常"
	LOG_RESULT_ABNORMAL = "異常"
	LOG_RESULT_ERROR    = "エラー"
)

func GetBQConnection() (*bqc.Connection, error) {

	keyFile := GConfig.StrOrEmpty("PemKeyFile")
	mailAccount := GConfig.StrOrEmpty("ServiceAccountMail")
	projectId := GConfig.StrOrEmpty("ProjectId")

	retryCount := 0
	for {
		conn, err := bqc.NewConnection(keyFile, mailAccount, projectId)
		if err == nil {
			return conn, nil
		}
		retryCount++
		if retryCount > MaxRetry {
			return nil, exterror.WrapExtError(err)
		}
		if strings.Contains(err.Error(), "Please try again in ") {
			time.Sleep(30 * time.Second)
		} else {
			time.Sleep(3 * time.Second)
		}
	}
}

func GetBQUtil() (*bq.Util, error) {

	retryCount := 0
	for {
		bqu, err := gcp.NewBQUtil(&gcp.GCPConfig{
			PrivateKeyPem: GConfig.StrOrEmpty("PemKeyFile"),
			Email:         GConfig.StrOrEmpty("ServiceAccountMail"),
			ProjectID:     GConfig.StrOrEmpty("ProjectId"),
		})
		if err == nil {
			return bqu, nil
		}

		retryCount++
		if retryCount > MaxRetry {
			return nil, exterror.WrapExtError(err)
		}
		if strings.Contains(err.Error(), "Please try again in ") {
			time.Sleep(30 * time.Second)
		} else {
			time.Sleep(3 * time.Second)
		}
	}
}

func GetGSUtil() (*st.Util, error) {
	retryCount := 0
	for {
		gsu, err := gcp.NewGSUtil(&gcp.GCPConfig{
			PrivateKeyPem: GConfig.StrOrEmpty("PemKeyFile"),
			Email:         GConfig.StrOrEmpty("ServiceAccountMail"),
			ProjectID:     GConfig.StrOrEmpty("ProjectId"),
		})
		if err == nil {
			return gsu, nil
		}

		retryCount++
		if retryCount > MaxRetry {
			return nil, exterror.WrapExtError(err)
		}
		if strings.Contains(err.Error(), "Please try again in ") {
			time.Sleep(30 * time.Second)
		} else {
			time.Sleep(3 * time.Second)
		}
		time.Sleep(3 * time.Second)
	}
}

func GetDBUtil() (*db.Util, error) {

	u := db.Util{
		Driver: GConfig.StrOrEmpty(CfgDBDriver),
		Host:   GConfig.StrOrEmpty(CfgDBHost),
		Port:   GConfig.IntOrZero(CfgDBPort),
		User:   GConfig.StrOrEmpty(CfgDBUser),
		Pwd:    GConfig.StrOrEmpty(CfgDBPwd),
		DBName: GConfig.StrOrEmpty(CfgDBName),
	}
	if err := u.Connect(); err != nil {
		return nil, exterror.WrapExtError(err)
	}

	return &u, nil
}

func UpLoadToGCS(arrFileUpload []string, gcsDir string, deleteLocal bool) error {
	PrintLog("Processing Upload file to GCS start")
	defer PrintLog("Processing Upload file to GCS end")

	gsu, err := GetGSUtil()
	if err != nil {
		return exterror.WrapExtError(err)
	}

	for _, filePath := range arrFileUpload {
		result := strings.Split(filePath, "/")
		PrintLog("Waiting For File " + result[len(result)-1] + " is Uploaded To GCS")

		gulErrCh := gsu.UploadFile(filePath, gcsDir)
		if gulErrCh != nil {
			return exterror.WrapExtError(gulErrCh)
		}

		if deleteLocal {
			os.Remove(filePath)
		}
	}

	return nil
}

func MoveFileGCS(arrFileUpload []string, gcsDir string) error {
	PrintLog("Processing move file in GCS start")
	defer PrintLog("Processing move file in GCS end")

	gsu, err := GetGSUtil()
	if err != nil {
		return exterror.WrapExtError(err)
	}

	for _, filePath := range arrFileUpload {
		result := strings.Split(filePath, "/")
		PrintLog("Waiting For File " + result[len(result)-1] + " move")

		gulErrCh := gsu.MoveFile(filePath, gcsDir)
		if gulErrCh != nil {
			return exterror.WrapExtError(gulErrCh)
		}
	}

	return nil
}

func SendMail(subject string, content string, jobname string, address []string) {
	// Get config mail info
	if GConfig.BoolOrFalse(CfgMailEnable) {
		mailSender := mailsender.MailSender{
			Server: GConfig.StrOrEmpty(CfgMailServer),
			Port:   GConfig.StrOrEmpty(CfgMailPort),
			User:   GConfig.StrOrEmpty(CfgMailUser),
			Pass:   GConfig.StrOrEmpty(CfgMailPwd),
			From: mail.Address{
				Name:    GConfig.StrOrEmpty(CfgMailSenderName),
				Address: GConfig.StrOrEmpty(CfgMailSender),
			},
		}

		for i := range address {
			if address[i] == "" {
				return
			}
			mailTo := mail.Address{
				Address: strings.TrimSpace(address[i]),
			}
			errSend := mailSender.SendText(mailTo, subject, content)
			if errSend != nil {
				log.Printf("ジョブ「%v」 の ERROR : メール送信に失敗しました。[%v( %v : From %v , To %v)]\n",
					jobname,
					errSend.Error(),
					mailSender.Server,
					mailSender.From.Address,
					mailTo.Address)
			}
		}
	}
}

func SendTxtToFTP(ftpAddress string, ftpUser string, ftpPwd string, localFile string, uploadDir string) *ErrorDetail {
	ftpCon, err := ftp.Connect(ftpAddress)
	if err != nil {
		err = exterror.TraceError(err)
		return &ErrorDetail{
			err,
			FAIL_CAUSE_FTP_CONNECT,
		}
	}

	err = ftpCon.Login(ftpUser, ftpPwd)
	if err != nil {
		err = exterror.TraceError(err)
		return &ErrorDetail{
			err,
			FAIL_CAUSE_FTP_LOGIN,
		}
	}
	defer ftpCon.Logout()
	err = ftpCon.ChangeDir(uploadDir)
	if err != nil {
		err = exterror.TraceError(err)
		return &ErrorDetail{
			err,
			FAIL_CAUSE_FTP_UPLOAD,
		}
	}

	rd, err := os.Open(localFile)
	if err != nil {
		err = exterror.TraceError(err)
		return &ErrorDetail{
			err,
			FAIL_CAUSE_FTP_UPLOAD,
		}
	}

	err = ftpCon.Stor(path.Join(uploadDir, filepath.Base(localFile)), rd)
	if err != nil {
		rd.Close()

		err = exterror.TraceError(err)
		return &ErrorDetail{
			err,
			FAIL_CAUSE_GCS_UPLOAD,
		}
	}

	rd.Close()
	os.Remove(localFile)

	return nil

}
