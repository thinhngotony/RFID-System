package bq

import (
	"errors"
	"fmt"
	"regexp"
	"strings"
	"time"

	"github.com/goframework/gf/exterror"
	"google.golang.org/api/bigquery/v2"
)

const (
	WriteTruncate   = "WRITE_TRUNCATE"
	WriteAppend     = "WRITE_APPEND"
	WriteEmpty      = "WRITE_EMPTY"
	StatusStateDone = "DONE"

	ErrorMayRetrying    = "Retrying may solve the problem."
	ErrorUnableComplete = "The job encountered an internal error during execution and was unable to complete successfully"
)

type Util struct {
	*bigquery.Service
	ProjectId string
}

type FieldSchema struct {
	Name    string
	Type    string
	Comment string
}

func (u *Util) TableExist(datasetId string, tableId string) bool {
	r, err := u.Tables.Get(u.ProjectId, datasetId, tableId).Do()
	if err != nil {
		return false
	}
	return r != nil
}

func (u *Util) ViewExist(datasetId string, viewId string) bool {
	r, err := u.Tables.Get(u.ProjectId, datasetId, viewId).Do()
	if err != nil {
		return false
	}
	return r != nil && r.View != nil
}

func (u *Util) GetViewDetail(datasetId string, viewId string) string {
	r, err := u.Tables.Get(u.ProjectId, datasetId, viewId).Do()
	if err == nil && r != nil && r.View != nil {
		return r.View.Query
	}

	return ""
}

func (u *Util) CreateView(datasetId string, viewId string, sqlView string) error {
	viewInfo := &bigquery.Table{
		TableReference: &bigquery.TableReference{
			DatasetId: datasetId,
			ProjectId: u.ProjectId,
			TableId:   viewId,
		},
		Id: viewId,
		View: &bigquery.ViewDefinition{
			Query: sqlView,
		},
	}
	c := u.Tables.Insert(u.ProjectId, datasetId, viewInfo)
	_, err := c.Do()
	return exterror.WrapExtError(err)
}

func (u *Util) DeleteTable(datasetId string, tableId string) error {
	return u.Tables.Delete(u.ProjectId, datasetId, tableId).Do()
}

func (u *Util) ListTables(datasetId string, prefix string) ([]string, error) {
	var r []string
	pageToken := ""
	tableStartIndex := len(u.ProjectId) + 1 + len(datasetId) + 1

	for {
		list, err := u.Tables.List(u.ProjectId, datasetId).PageToken(pageToken).Do()
		if err != nil {
			return nil, err
		}
		for _, t := range list.Tables {
			tableId := t.Id[tableStartIndex:]
			if strings.HasPrefix(tableId, prefix) {
				r = append(r, tableId)
			}
		}
		pageToken = list.NextPageToken
		if pageToken == "" {
			break
		}
	}

	return r, nil
}

func (u *Util) ListTablesRegex(datasetId string, regex string) ([]string, error) {
	var r []string
	pageToken := ""
	tableStartIndex := len(u.ProjectId) + 1 + len(datasetId) + 1

	reg, err := regexp.Compile(regex)
	if err != nil {
		return nil, err
	}
	for {
		list, err := u.Tables.List(u.ProjectId, datasetId).PageToken(pageToken).Do()
		if err != nil {
			return nil, err
		}
		for _, t := range list.Tables {
			tableId := t.Id[tableStartIndex:]
			if reg.MatchString(tableId) {
				r = append(r, tableId)
			}
		}
		pageToken = list.NextPageToken
		if pageToken == "" {
			break
		}
	}

	return r, nil
}

func (u *Util) CopyTable(fromDatasetId string, fromTableId string, toDatasetId string, toTableId string, override bool) error {

	var writeMode string
	if override {
		writeMode = WriteTruncate
	} else {
		writeMode = WriteEmpty
	}

	job := bigquery.Job{
		JobReference: &bigquery.JobReference{ProjectId: u.ProjectId},
		Configuration: &bigquery.JobConfiguration{
			Copy: &bigquery.JobConfigurationTableCopy{
				WriteDisposition: writeMode,
				DestinationTable: &bigquery.TableReference{
					TableId:   toTableId,
					DatasetId: toDatasetId,
					ProjectId: u.ProjectId,
				},
				SourceTable: &bigquery.TableReference{
					TableId:   fromTableId,
					DatasetId: fromDatasetId,
					ProjectId: u.ProjectId,
				},
			},
		},
	}

	jobResult, err := u.Jobs.Insert(u.ProjectId, &job).Do()
	if err != nil {
		return exterror.WrapExtError(err)
	}

	for {
		var err error
		response, err := u.Jobs.Get(jobResult.JobReference.ProjectId, jobResult.JobReference.JobId).Do()
		if err != nil {
			return exterror.WrapExtError(err)
		}
		if len(response.Status.Errors) > 0 {
			return errors.New(response.Status.Errors[0].Message)
		}
		if response.Status.State == StatusStateDone {
			break
		}

		time.Sleep(time.Millisecond * 500)
	}

	return nil
}

func (u *Util) MoveTable(fromDatasetId string, fromTableId string, toDatasetId string, toTableId string, override bool) error {
	err := u.CopyTable(fromDatasetId, fromTableId, toDatasetId, toTableId, override)
	if err != nil {
		return exterror.WrapExtError(err)
	}
	return u.DeleteTable(fromDatasetId, fromTableId)
}

func (u *Util) Execute(query string) error {
	useQueryCache := false

	job := bigquery.Job{
		JobReference: &bigquery.JobReference{ProjectId: u.ProjectId},
		Configuration: &bigquery.JobConfiguration{
			Query: &bigquery.JobConfigurationQuery{
				Query:         query,
				UseQueryCache: &useQueryCache,
			},
		},
	}

	return u.ExecuteJob(&job)
}

func (u *Util) ExecuteAppend(query string, dataset string, table string) error {
	useQueryCache := false

	job := bigquery.Job{
		JobReference: &bigquery.JobReference{ProjectId: u.ProjectId},
		Configuration: &bigquery.JobConfiguration{
			Query: &bigquery.JobConfigurationQuery{
				Query:         query,
				UseQueryCache: &useQueryCache,
				DestinationTable: &bigquery.TableReference{
					DatasetId: dataset,
					TableId:   table,
					ProjectId: u.ProjectId,
				},
				AllowLargeResults: true,
				WriteDisposition:  WriteAppend,
			},
		},
	}

	return u.ExecuteJob(&job)
}
func (u *Util) ExecuteOverwrite(query string, dataset string, table string) error {
	useQueryCache := false

	job := bigquery.Job{
		JobReference: &bigquery.JobReference{ProjectId: u.ProjectId},
		Configuration: &bigquery.JobConfiguration{
			Query: &bigquery.JobConfigurationQuery{
				Query:         query,
				UseQueryCache: &useQueryCache,
				DestinationTable: &bigquery.TableReference{
					DatasetId: dataset,
					TableId:   table,
					ProjectId: u.ProjectId,
				},
				AllowLargeResults: true,
				WriteDisposition:  WriteTruncate,
			},
		},
	}

	return u.ExecuteJob(&job)
}

func (u *Util) ExecuteJob(job *bigquery.Job) error {
	var jr *bigquery.Job
	var err error
	var retryJob = false

BeginExecuteJob:
	retryJob = false
	for {
		jr, err = u.Jobs.Insert(u.ProjectId, job).Do()
		if err == nil {
			break
		} else if strings.Contains(err.Error(), ErrorMayRetrying) {
			continue
		} else {
			return exterror.WrapExtError(err)
		}
	}

	if err != nil {
		return exterror.WrapExtError(err)
	}

	var qr *bigquery.GetQueryResultsResponse
	for {
		var err error
		qr, err = u.Jobs.GetQueryResults(jr.JobReference.ProjectId, jr.JobReference.JobId).MaxResults(1).Do()

		if err != nil {
			if strings.Contains(err.Error(), ErrorMayRetrying) {
				continue
			} else if strings.Contains(err.Error(), ErrorUnableComplete) {
				retryJob = true
				break
			} else {
				return exterror.WrapExtError(err)
			}
		}

		if qr.JobComplete {
			break
		}

		time.Sleep(500 * time.Millisecond)
	}

	if retryJob {
		time.Sleep(500 * time.Millisecond)
		goto BeginExecuteJob
	}

	return nil
}

func (u *Util) ImportFromGS(gcsFile string, toDataset string, toTable string, writeMode string, skipLeadingRows int, fieldDelimiter string, schema []FieldSchema) error {
	if len(schema) <= 0 {
		return errors.New("schema is required")
	}

	var fields []*bigquery.TableFieldSchema
	for _, s := range schema {
		fields = append(fields, &bigquery.TableFieldSchema{
			Description: s.Comment,
			Name:        s.Name,
			Type:        s.Type,
		})
	}

	job := bigquery.Job{
		JobReference: &bigquery.JobReference{ProjectId: u.ProjectId},
		Configuration: &bigquery.JobConfiguration{
			Load: &bigquery.JobConfigurationLoad{
				SkipLeadingRows: int64(skipLeadingRows),
				FieldDelimiter:  fieldDelimiter,
				DestinationTable: &bigquery.TableReference{
					ProjectId: u.ProjectId,
					DatasetId: toDataset,
					TableId:   toTable,
				},
				WriteDisposition:    writeMode,
				AllowQuotedNewlines: true,
				Schema: &bigquery.TableSchema{
					Fields: fields,
				},
				SourceUris: []string{"gs://" + gcsFile},
			},
		},
	}

	var jr *bigquery.Job
	var err error
	for {
		jr, err = u.Jobs.Insert(u.ProjectId, &job).Do()
		if err == nil {
			break
		} else if strings.Contains(err.Error(), ErrorMayRetrying) {
			continue
		} else {
			return exterror.WrapExtError(err)
		}
	}

	for {
		var err error
		response, err := u.Jobs.Get(jr.JobReference.ProjectId, jr.JobReference.JobId).Do()
		if err != nil {
			if strings.Contains(err.Error(), ErrorMayRetrying) {
				continue
			} else {
				return exterror.WrapExtError(err)
			}
		}

		if len(response.Status.Errors) > 0 {
			errMsg := ""
			for _, e := range response.Status.Errors {
				errMsg = errMsg + fmt.Sprintf("%+v", e) + "\r\n"
			}
			return errors.New(errMsg)
		}
		if response.Status.State == StatusStateDone {
			break
		}

		time.Sleep(time.Millisecond * 500)
	}

	return nil
}
