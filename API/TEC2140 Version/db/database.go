package db

import (
	"compress/gzip"
	"database/sql"
	"encoding/csv"
	"fmt"
	"io"
	"os"
	"strings"

	"github.com/go-sql-driver/mysql"
	_ "github.com/go-sql-driver/mysql"
)

type FieldDef struct {
	Name    string
	Type    string
	Comment string
}

type Util struct {
	Driver string
	Host   string
	Port   int
	User   string
	Pwd    string
	DBName string

	DB *sql.DB
}

func (du *Util) Connect() error {
	db, err := sql.Open(du.Driver, du.User+":"+du.Pwd+"@tcp("+du.Host+":"+fmt.Sprint(du.Port)+")/"+du.DBName)
	if err != nil {
		return err
	}
	du.DB = db

	return nil
}

func (du *Util) ListTables(like string) ([]string, error) {

	var list []string

	query := "SHOW TABLES WHERE Tables_in_" + du.DBName + " LIKE ?"

	rows, err := du.DB.Query(query, like)
	if err != nil {
		return nil, err
	}

	for rows.Next() {
		var tableName string
		err = rows.Scan(&tableName)
		if err != nil {
			return nil, err
		}

		list = append(list, tableName)
	}

	rows.Close()

	return list, nil
}

func (du *Util) GetTableFieldDef(table string) ([]FieldDef, error) {
	var list []FieldDef

	query := "SHOW FULL COLUMNS FROM " + table

	rows, err := du.DB.Query(query)
	if err != nil {
		return nil, err
	}

	for rows.Next() {
		var dump *string
		var comment *string

		col := FieldDef{}
		err = rows.Scan(&col.Name, &col.Type, &dump, &dump, &dump, &dump, &dump, &dump, &comment)
		if err != nil {
			return nil, err
		}
		if comment != nil {
			col.Comment = *comment
		}
		if strings.HasPrefix(col.Type, "tinyint") ||
			strings.HasPrefix(col.Type, "smallint") ||
			strings.HasPrefix(col.Type, "int") ||
			strings.HasPrefix(col.Type, "bigint") ||
			strings.HasPrefix(col.Type, "bit") {
			col.Type = "INTEGER"
		} else if strings.HasPrefix(col.Type, "float") ||
			strings.HasPrefix(col.Type, "double") ||
			strings.HasPrefix(col.Type, "decimal") {
			col.Type = "FLOAT"
		} else {
			col.Type = "STRING"
		}

		list = append(list, col)
	}

	rows.Close()

	return list, nil
}

func (du *Util) TableToCsv(table string, csvFile string) error {

	schema, err := du.GetTableFieldDef(table)
	if err != nil {
		return err
	}
	var header []string
	for _, col := range schema {
		header = append(header, col.Name)
	}

	query := "SELECT * FROM " + table

	rows, err := du.DB.Query(query)
	if err != nil {
		return err
	}
	defer rows.Close()

	file, err := os.Create(csvFile)
	if err != nil {
		return err
	}

	var csvWriter *csv.Writer
	var gzWriter *gzip.Writer

	if strings.HasSuffix(strings.ToLower(csvFile), ".gz") {
		gzWriter = gzip.NewWriter(file)
		csvWriter = csv.NewWriter(gzWriter)
	} else {
		csvWriter = csv.NewWriter(file)
	}

	csvWriter.UseCRLF = true
	csvWriter.Write(header)

	cols, err := rows.Columns()
	if err != nil {
		return err
	}

	countCols := len(cols)

	values := make([]interface{}, countCols)
	valuePtrs := make([]interface{}, countCols)
	valueStrs := make([]string, countCols)

	for rows.Next() {
		for i := 0; i < countCols; i++ {
			valuePtrs[i] = &values[i]
		}

		err := rows.Scan(valuePtrs...)
		if err != nil {
			return err
		}

		for i, val := range values {

			b, ok := val.([]byte)

			if ok {
				valueStrs[i] = string(b)
			} else {
				valueStrs[i] = ""
			}
		}

		csvWriter.Write(valueStrs)
	}
	csvWriter.Flush()
	if gzWriter != nil {
		gzWriter.Flush()
		gzWriter.Close()
	}

	file.Close()

	return nil
}

func (du *Util) StreamExport(table string, wr io.WriteCloser, gzipMode bool, customQuery string) error {

	schema, err := du.GetTableFieldDef(table)
	if err != nil {
		return err
	}
	var header []string
	for _, col := range schema {
		header = append(header, col.Name)
	}

	query := "SELECT * FROM " + table
	if customQuery != "" {
		cq := strings.TrimSpace(strings.ToLower(customQuery))
		if strings.HasPrefix(cq, "select") {
			query = customQuery
		} else if strings.HasPrefix(cq, "where") {
			query += "\n" + customQuery
		}
	}

	rows, err := du.DB.Query(query)
	if err != nil {
		return err
	}
	defer rows.Close()

	var csvWriter *csv.Writer
	var gzWriter *gzip.Writer

	if gzipMode {
		gzWriter = gzip.NewWriter(wr)
		csvWriter = csv.NewWriter(gzWriter)
	} else {
		csvWriter = csv.NewWriter(wr)
	}

	csvWriter.UseCRLF = true
	csvWriter.Write(header)

	cols, err := rows.Columns()
	if err != nil {
		return err
	}

	countCols := len(cols)

	values := make([]interface{}, countCols)
	valuePtrs := make([]interface{}, countCols)
	valueStrs := make([]string, countCols)

	for rows.Next() {
		for i := 0; i < countCols; i++ {
			valuePtrs[i] = &values[i]
		}

		err := rows.Scan(valuePtrs...)
		if err != nil {
			return err
		}

		for i, val := range values {

			b, ok := val.([]byte)

			if ok {
				valueStrs[i] = string(b)
			} else {
				valueStrs[i] = ""
			}
		}

		csvWriter.Write(valueStrs)
	}
	csvWriter.Flush()
	if gzWriter != nil {
		gzWriter.Flush()
		gzWriter.Close()
	}

	wr.Close()

	return nil
}

// Load csv (csv.gz) file to a table
func (du *Util) TableFromCsv(table string, csvFile string, truncate bool, listField []string) error {

	var err error

	var query string
	if truncate {
		query = `TRUNCATE TABLE ` + table
		_, err := du.DB.Exec(query)
		if err != nil {
			return err
		}
	}

	fileReader, err := os.Open(csvFile)
	if err != nil {
		return err
	}
	defer fileReader.Close()

	if strings.HasSuffix(strings.ToLower(csvFile), ".gz") {
		gzReader, err := gzip.NewReader(fileReader)
		if err != nil {
			return err
		}
		defer gzReader.Close()

		mysql.RegisterReaderHandler(table, func() io.Reader { return gzReader })
	} else {
		mysql.RegisterReaderHandler(table, func() io.Reader { return fileReader })
	}

	defer mysql.DeregisterReaderHandler(table)

	query = `
LOAD DATA LOW_PRIORITY LOCAL INFILE 'Reader::` + table + `'
REPLACE INTO TABLE ` + table + `
CHARACTER SET utf8
FIELDS TERMINATED BY ','
OPTIONALLY ENCLOSED BY '"'
ESCAPED BY '"'
LINES TERMINATED BY '\r\n'
IGNORE 1 LINES
`
	if len(listField) > 0 {
		query = query + "(" + strings.Join(listField, ",") + ")"
	}

	_, err = du.DB.Exec(query)
	if err != nil {
		return err
	}

	return nil
}

// Load data from a stream to table, listField is optional (nil OK)
func (du *Util) StreamImport(toTable string, ignoreLines int, fieldTerminate string, endOfLine string, rd io.Reader, truncate bool, listField []string) error {

	var err error

	var query string
	if truncate {
		query = `TRUNCATE TABLE ` + toTable
		_, err := du.DB.Exec(query)
		if err != nil {
			return err
		}
	}

	mysql.RegisterReaderHandler(toTable, func() io.Reader { return rd })
	defer mysql.DeregisterReaderHandler(toTable)

	query = `
LOAD DATA LOW_PRIORITY LOCAL INFILE 'Reader::` + toTable + `'
REPLACE INTO TABLE ` + toTable + `
CHARACTER SET utf8
FIELDS TERMINATED BY '` + fmt.Sprint(fieldTerminate) + `'
OPTIONALLY ENCLOSED BY '"'
ESCAPED BY '"'
LINES TERMINATED BY '` + fmt.Sprint(endOfLine) + `'
IGNORE ` + fmt.Sprint(ignoreLines) + ` LINES
`
	if len(listField) > 0 {
		query = query + "(" + strings.Join(listField, ",") + ")"
	}

	_, err = du.DB.Exec(query)
	if err != nil {
		return err
	}

	return nil
}

func (du *Util) Close() {
	if du.DB != nil {
		du.DB.Close()
		du.DB = nil
	}
}
