package Common

import (
	"github.com/goframework/gf/cfg"
)

var GConfig = cfg.Cfg{}

const (
	CONFIG_FILE = "config/config.cfg"

	CfgGCPKeyFile            = "PemKeyFile"
	CfgGCPServiceAccountMail = "ServiceAccountMail"
	CfgGCPProjectId          = "ProjectId"

	CfgDBDriver = "Database.Driver"
	CfgDBHost   = "Database.Host"
	CfgDBPort   = "Database.Port"
	CfgDBUser   = "Database.User"
	CfgDBPwd    = "Database.Pwd"
	CfgDBName   = "Database.DatabaseName"

	CfgJobMaxRetry           = "Job.RetryTimes"
	CfgJobRetryWaitTime      = "Job.WaitForRetry"
	CfgFtpRetryTimes         = "Ftp.RetryTimes"
	CfgFtpWaitForRetry       = "Ftp.WaitForRetry"
	CfgGSCUploadRetryTimes   = "GSC.UploadRetryTimes"
	CfgGSCUploadWaitForRetry = "GSC.UploadWaitForRetry"

	CfgJobLoopCycleTimeInMinuteJob01            = "Job.LoopCycleTimeInMinute.Job01"
	CfgJobLoopCycleTimeInMinuteJob02            = "Job.LoopCycleTimeInMinute.Job02"
	CfgJobLoopCycleTimeInMinuteJob03            = "Job.LoopCycleTimeInMinute.Job03"
	CfgJobLoopCycleTimeInMinuteJob04            = "Job.LoopCycleTimeInMinute.Job04"
	CfgJobLoopCycleTimeInMinuteJob05            = "Job.LoopCycleTimeInMinute.Job05"
	CfgJobLoopCycleTimeInMinuteJob90            = "Job.LoopCycleTimeInMinute.Job90"
	CfgJobLoopCycleTimeInMinuteJob91            = "Job.LoopCycleTimeInMinute.Job91"
	CfgJobLoopCycleTimeInMinuteJob99            = "Job.LoopCycleTimeInMinute.Job99"
	CfgJobLoopCycleTimeInMinuteGetBestSellerJob = "Job.LoopCycleTimeInMinute.GetBestSellerJob"

	CfgMailEnable       = "MAIL.Enable"
	CfgMailServer       = "MAIL.Server"
	CfgMailPort         = "MAIL.Port"
	CfgMailUser         = "MAIL.User"
	CfgMailPwd          = "MAIL.Pass"
	CfgMailSender       = "MAIL.Sender"
	CfgMailSenderName   = "MAIL.SenderName"
	CfgMailReceiver     = "MAIL.Receiver"
	CfgMailReceiverName = "MAIL.ReceiverName"

	CfgGSTmpDir            = "GS.TmpDir"
	CfgGSDatOutputDir      = "GS.DatOutputDir"
	CfgGSDatOutputDirBack  = "GS.DatOutputDirBack"
	CfgBQDataset           = "BQ.Dataset"
	CfgBQSrcDataset        = "BQ.SrcDataset"
	CfgBQDatasetBestSeller = "BQ.DatasetBestSeller"

	CfgDataDir     = "DataDir"
	CfgDataTempDir = "DataTempDir"

	CfgDatOutputDataDir  = "CfgDatOutputDataDir"
	CfgOutput002DataFile = "CfgOutput002DataFile"
	CfgOutput003DataFile = "CfgOutput003DataFile"

	CfgFtpAddress   = "CfgFtpAddress"
	CfgFtpUser      = "CfgFtpUser"
	CfgFtpPwd       = "CfgFtpPwd"
	CfgFtpUploadDir = "CfgFtpUploadDir"

	CfgOutOfStockDeleteDay      = "OutOfStockDeleteDay"
	CfgOutOfStockBackupDataFlag = "OutOfStockBackupDataFlag"
)

func init() {
	GConfig.Load(CONFIG_FILE)
}
