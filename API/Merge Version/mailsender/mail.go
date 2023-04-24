package mailsender

import (
	"bytes"
	"mime"
	"net/mail"
	"net/smtp"
)

type MailSender struct {
	Server string
	Port   string
	User   string
	Pass   string

	From mail.Address
}

const TEXT_PLAIN = "text/plain"
const TEXT_HTML = "text/html"

func (this *MailSender) Send(to mail.Address, subject string, content string, contentType string) error {
	if this.Server == "" && this.Port == "" {
		return nil
	}

	var auth smtp.Auth = nil
	if this.User != "" {
		auth = smtp.PlainAuth(
			"GoMailSenderAuth",
			this.User,
			this.Pass,
			this.Server,
		)
	}

	addr := this.Server + ":" + this.Port

	header := "From: " + this.From.String() + "\r\n" +
		"To: " + to.String() + "\r\n" +
		"Subject: " + mime.BEncoding.Encode("UTF-8", subject) + "\r\n" +
		"MIME-Version: 1.0\r\n" +
		"Content-Type: " + contentType + "; charset=UTF-8\r\n" +
		"Content-Transfer-Encoding: binary\r\n" +
		"\r\n"

	msgBuf := bytes.Buffer{}
	msgBuf.WriteString(header)
	msgBuf.WriteString(content)

	e := smtp.SendMail(addr, auth, this.From.Address, []string{to.Address}, msgBuf.Bytes())

	return e
}

func (this *MailSender) SendText(to mail.Address, subject string, content string) error {
	return this.Send(to, subject, content, TEXT_PLAIN)
}

func (this *MailSender) SendHtml(to mail.Address, subject string, content string) error {
	return this.Send(to, subject, content, TEXT_HTML)
}
