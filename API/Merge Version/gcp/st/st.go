package st

import (
	"google.golang.org/api/storage/v1"
	"io"
	"os"
	"path"
	"path/filepath"
	"strings"
)

const DownloadBufferSize = 16 * 1024

type Util struct {
	*storage.Service
	ProjectId string
}

//Move file from oldFileName to newFileName. File names is full path, include bucket name
func (u *Util) MoveFile(oldFileName string, newFileName string) error {
	err := u.CopyFile(oldFileName, newFileName)
	if err == nil {
		err = u.DeleteFile(oldFileName)
	}
	return err
}

//Copy file in current directory to another directory
func (u *Util) CopyFile(sourceFile string, destFile string) error {
	fromBucket := sourceFile[:strings.Index(sourceFile, "/")]
	toBucket := destFile[:strings.Index(destFile, "/")]

	fromFilePath := sourceFile[len(fromBucket)+1:]
	toFilePath := destFile[len(toBucket)+1:]

	_, err := u.Objects.Copy(fromBucket, fromFilePath, toBucket, toFilePath, nil).Do()
	return err
}

//Delete files
func (u *Util) DeleteFile(sourceFile string) error {
	fromBucket := sourceFile[:strings.Index(sourceFile, "/")]
	fromFilePath := sourceFile[len(fromBucket)+1:]

	return u.Objects.Delete(fromBucket, fromFilePath).Do()
}

//Download file
func (u *Util) DownloadStream(bucket, object string) (io.ReadCloser, error) {
	res, err := u.Objects.Get(bucket, object).Download()
	if err != nil {
		return nil, err
	}

	return res.Body, nil
}

//Download file
func (u *Util) DownloadFile(sourceFile string, fullLocalFile string) error {
	fromBucket := sourceFile[:strings.Index(sourceFile, "/")]
	fromFilePath := sourceFile[len(fromBucket)+1:]

	res, err := u.Objects.Get(fromBucket, fromFilePath).Download()
	if err != nil {
		return nil
	}

	defer res.Body.Close()

	file, err := os.Create(fullLocalFile)
	if err != nil {
		return err
	}
	defer file.Close()

	buffer := make([]byte, DownloadBufferSize)

	for {
		readSize, err := res.Body.Read(buffer)
		if readSize > 0 {
			file.Write(buffer[:readSize])
		}
		if err != nil {
			if err == io.EOF {
				break
			} else {
				return err
			}
		}
	}

	return nil
}

//Upload local file to GCS directory
func (u *Util) UploadFile(localFile string, gcsDir string) error {
	bucket := gcsDir[:strings.Index(gcsDir, "/")]
	gcsDir = gcsDir[len(bucket)+1:]

	// Insert an object into a bucket.
	objectName := gcsDir + "/" + filepath.Base(localFile)
	object := &storage.Object{Name: objectName}
	file, err := os.Open(localFile)
	if err != nil {
		return err
	}

	defer file.Close()

	if _, err := u.Objects.Insert(bucket, object).Media(file).Do(); err != nil {
		return err
	}

	return nil
}

//Open upload stream to GS object
func (u *Util) OpenUploadStream(gsFilePath string) (io.WriteCloser, chan error, error) {
	bucket := gsFilePath[:strings.Index(gsFilePath, "/")]
	objectName := gsFilePath[len(bucket)+1:]

	// Insert an object into a bucket.
	object := &storage.Object{Name: objectName}

	pipeReader, pipeWriter := io.Pipe()

	resultChan := make(chan error)
	go func() {
		if _, err := u.Objects.Insert(bucket, object).Media(pipeReader).Do(); err != nil {
			resultChan <- err
		} else {
			resultChan <- nil
		}
		close(resultChan)
	}()

	return pipeWriter, resultChan, nil
}

func (u *Util) List(bucket, prefix string) ([]string, error) {
	call := u.Objects.List(bucket).Prefix(prefix).Delimiter("/").IncludeTrailingDelimiter(false)
	res, err := call.Do()
	if err != nil {
		return nil, err
	}

	var listFile []string
	prefixLen := len(prefix)

	for {
		for _, obj := range res.Items {
			if len(obj.Name) > prefixLen {
				listFile = append(listFile, obj.Name)
			}
		}

		if res.NextPageToken != "" {
			call = call.PageToken(res.NextPageToken)
			res, err = call.Do()
			if err != nil {
				return nil, err
			}
		} else {
			break
		}
	}
	return listFile, nil
}

func (u *Util) ListFile(gsFolderPath string, suffix string) ([]string, error) {
	bucket := gsFolderPath[:strings.Index(gsFolderPath, "/")]
	objectName := gsFolderPath[len(bucket)+1:]

	call := u.Objects.List(bucket).Prefix(objectName)
	res, err := call.Do()
	if err != nil {
		return nil, err
	}

	var listFile []string
	prefixLen := len(objectName)

	for {
		for _, obj := range res.Items {
			if len(obj.Name) > prefixLen && strings.HasSuffix(obj.Name, suffix) {
				listFile = append(listFile, path.Join(bucket,obj.Name))
			}
		}

		if res.NextPageToken != "" {
			call = call.PageToken(res.NextPageToken)
			res, err = call.Do()
			if err != nil {
				return nil, err
			}
		} else {
			break
		}
	}
	return listFile, nil
}
