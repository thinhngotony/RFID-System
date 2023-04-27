# Setting up services

1. Copy all folders to `/etc/systemd/system/` directory.
2. Copy all source to a valid directory.
3. Run the following commands in order:

## Grant permission
```
sudo chmod 777 /RFID_Api/main
sudo chmod 777 /WPC/API/ShopGoodMasterApi.exe
sudo chmod 777 /WPC/DIGITAL_SIGNAGE/GetBestSellerJob.exe
sudo chmod 777 /WPC/DIGITAL_SIGNAGE/DigitalSignageGCEApi.exe
sudo chmod -R 777 /WPC/DIGITAL_SIGNAGE
sudo chmod 777 /WPC/DIGITAL_SIGNAGE_FE/node_modules/.bin/
sudo chmod 777 /WPC/DIGITAL_SIGNAGE_FE/.sh
sudo chmod 777 /WPC/DIGITAL_SIGNAGE_API/.sh
```

## Enable auto-start
```
sudo systemctl enable DigitalSignageGCEApi.service
sudo systemctl enable GetBestSellerJob.service
sudo systemctl enable ShopGoodMasterApi.service
sudo systemctl enable digital_signage_api.service
sudo systemctl enable digital_signage_fe.service
sudo systemctl enable RFID_Api.service
```

## Start services
```
sudo systemctl start DigitalSignageGCEApi.service
sudo systemctl start GetBestSellerJob.service
sudo systemctl start ShopGoodMasterApi
sudo systemctl start digital_signage_api.service
sudo systemctl start digital_signage_fe.service
sudo systemctl start RFID_Api.service
```

## Check status for each service
```
sudo systemctl status DigitalSignageGCEApi.service
sudo systemctl status GetBestSellerJob.service
sudo systemctl status ShopGoodMasterApi
sudo systemctl status digital_signage_api.service
sudo systemctl status digital_signage_fe.service
sudo systemctl status RFID_Api.service
```
