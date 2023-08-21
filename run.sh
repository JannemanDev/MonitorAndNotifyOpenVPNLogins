# Wait for openvpn to be started
until pids=$(pidof openvpn)
do   
    sleep 1
done

# line below is mandatory when starting from .conf file
export DOTNET_BUNDLE_EXTRACT_BASE_DIR=/tmp

# change below to installation directory
cd /volume1/homes/someuser/MonitorAndNotifyOpenVPNLogins-1.0-linux-arm-netcoreapp3.1
./MonitorAndNotifyOpenVPNLogins

# application output log can be found under /var/log/upstart/MonitorAndNotifyOpenVPNLogins.log
