package main

import (
	"main/controllers"

	"github.com/gin-gonic/gin"
	_ "github.com/go-sql-driver/mysql"
)

func main() {
	gin.SetMode(gin.ReleaseMode)
	r := gin.Default()
	r.POST("/api/v1/rfid_to_jan", controllers.GetJanCodefromRFID)
	r.POST("/api/v1/rfids_to_jans", controllers.GetJanCodefromRFIDList)
	r.POST("/api/v2/rfids_to_jans", controllers.GetJanCodefromRFIDList_2)
	r.POST("/api/v1/rfids_to_status", controllers.GetStatusfromRFIDList)
	r.POST("/api/v1/jan_to_rfid", controllers.GetRFIDfromJanCode)
	r.POST("/api/v1/insert_rfid_master", controllers.InsertDataToMasterTable)
	r.POST("/api/v1/delete_rfids_master", controllers.DeleteDataFromMasterTable)
	r.POST("/api/v1/insert_rfid_log", controllers.InsertDataToLogTable)
	r.POST("/api/v1/insert_rfid_logs", controllers.InsertMultiDataToLogTable)
	r.POST("/api/v1/search", controllers.SearchfromJan)
	r.POST("/api/v1/search_fromBQ", controllers.SearchFromJan_BQ)
	r.POST("/api/v1/rfid_to_info", controllers.GetInfofromRFID)
	if err := r.Run("0.0.0.0:8080"); err != nil {
		panic(err.Error())
	}
}
