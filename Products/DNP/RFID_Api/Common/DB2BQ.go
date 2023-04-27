package Common

import (
	"path"

	"main/gcp/bq"

	"github.com/goframework/gf/exterror"
)

func DBToBQ(fromTable string, destDataset string, customQuery string) *ErrorDetail {

	dbu, err := GetDBUtil()
	if err != nil {
		return &ErrorDetail{
			exterror.TraceError(err),
			FAIL_CAUSE_DB_CONNECT,
		}
	}
	defer dbu.DB.Close()

	gsu, err := GetGSUtil()
	if err != nil {
		return &ErrorDetail{
			exterror.TraceError(err),
			FAIL_CAUSE_GCS_CONNECT,
		}

	}

	gsTmpFile := path.Join(GConfig.StrOrEmpty(CfgGSTmpDir), fromTable+"_"+Rand32bitStr()+".csv.gz")
	gsw, gswErrCh, err := gsu.OpenUploadStream(gsTmpFile)
	if err != nil {
		return &ErrorDetail{
			exterror.TraceError(err),
			FAIL_CAUSE_GCS_UPLOAD,
		}
	}

	err = dbu.StreamExport(fromTable, gsw, true, customQuery)
	if err != nil {
		return &ErrorDetail{
			exterror.TraceError(err),
			FAIL_CAUSE_GCS_UPLOAD,
		}
	}

	err = <-gswErrCh
	if err != nil {
		return &ErrorDetail{
			exterror.TraceError(err),
			FAIL_CAUSE_GCS_UPLOAD,
		}
	}

	bqu, err := GetBQUtil()
	if err != nil {
		return &ErrorDetail{
			exterror.TraceError(err),
			FAIL_CAUSE_BQ_CONNECT,
		}
	}

	fieldsDef, err := dbu.GetTableFieldDef(fromTable)
	if err != nil {
		return &ErrorDetail{
			exterror.TraceError(err),
			FAIL_CAUSE_DB_QUERY,
		}
	}

	var fieldSchemas []bq.FieldSchema
	for _, f := range fieldsDef {
		fieldSchemas = append(fieldSchemas, bq.FieldSchema{
			Name:    f.Name,
			Type:    f.Type,
			Comment: f.Comment,
		})
	}

	toTable := fromTable
	skip1Rows := 1
	fieldDelimiter := ","
	err = bqu.ImportFromGS(gsTmpFile, destDataset, toTable, bq.WriteTruncate, skip1Rows, fieldDelimiter, fieldSchemas)
	if err != nil {
		return &ErrorDetail{
			exterror.TraceError(err),
			FAIL_CAUSE_BQ_IMPORT,
		}
	}

	gsu.DeleteFile(gsTmpFile)

	return nil
}
