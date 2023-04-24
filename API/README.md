# Hướng dẫn cài đặt và sử dụng RFID API

## Chuẩn bị môi trường

Giải nén và chép tất cả dữ liệu trong "ProductManage.zip" vào thư mục của Golang(GOROOT) trong Windows: C:\Program Files\Go\src

Nếu không có file source hãy tải file "ProductManage.zip" về từ [đây](<https://drive.google.com/drive/folders/1JFF1JeGn8VEBCpN32CzO3IF4sV7YL1Mj?usp=share_link>)

## Mở Port

Mở Port bằng các lệnh sau:

```
sudo ufw allow 8027
sudo ufw reload
firewall-cmd --add-port=8027/tcp
firewall-cmd --permanent --add-port=8027/tcp
```

## Sao chép source code

Sao chép source code API vào thư mục /RFID_Api/master

## Tạo file service

Tạo file `RFID_Api.service` và chép vào thư mục /etc/systemd/system/ với nội dung như sau:

```
[Unit]
Description=RFID_API

[Service]
User=root
WorkingDirectory=/RFID_Api/master
ExecStart=/RFID_Api/master/main
Restart=always

[Install]
WantedBy=multi-user.target
```

## Khởi động dịch vụ

Chạy các lệnh sau để khởi động dịch vụ:

```
chmod 777 /RFID_Api/main/main
sudo systemctl start RFID_Api.service
sudo systemctl enable RFID_Api.service
```

Chạy các lệnh sau để kiểm tra trạng thái dịch vụ:

```
sudo systemctl status RFID_Api.service
```

Chạy các lệnh sau để dừng dịch vụ:

```
sudo systemctl stop RFID_Api.service
sudo systemctl disable RFID_Api.service
```

## Cách dùng

Sử dụng POSTMAN:

- Postman: Dùng lệnh POST IP:8027/api/v1/rfid_to_jan.
