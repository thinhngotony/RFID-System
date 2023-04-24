# Hướng dẫn cài đặt và sử dụng RFID API

## Chuẩn bị môi trường

Giải nén và chép tất cả dữ liệu trong "ProductManage.zip" vào thư mục của Golang(GOROOT) trong Windows: C:\Program Files\Go\src

Nếu không có file source hãy tải file "ProductManage.zip" về từ [đây](<https://drive.google.com/drive/folders/1JFF1JeGn8VEBCpN32CzO3IF4sV7YL1Mj?usp=share_link>)

## Mở Port

Mở Port bằng các lệnh sau:

sudo ufw allow 8027
sudo ufw reload
firewall-cmd --add-port=8027/tcp
firewall-cmd --permanent --add-port=8027/tcp

## Sao chép source code

Sao chép source code API vào thư mục /RFID_Api/master

## Tạo file service

Tạo file RFID_Api_Lastest.service và chép vào thư mục /etc/systemd/system/ với nội dung như sau:

[Unit]
Description=RFID_API

[Service]
User=root
WorkingDirectory=/RFID_Api/master
ExecStart=/RFID_Api/master/main
Restart=always

[Install]
WantedBy=multi-user.target

## Khởi động dịch vụ

Chạy các lệnh sau để khởi động dịch vụ:

chmod 777 /RFID_Api/main/main
sudo systemctl stop RFID_Api_Lastest.service
sudo systemctl start RFID_Api_Lastest.service
sudo systemctl enable RFID_Api_Lastest.service
sudo systemctl disable RFID_Api_Lastest.service

## Kiểm tra dịch vụ

Sử dụng câu lệnh sau để kiểm tra dịch vụ:

- Terminal: sudo systemctl status RFID_Api.service => Trả về running nếu thành công.
- Postman: Dùng lệnh GET IP:8027/api/v1/xxxx.
