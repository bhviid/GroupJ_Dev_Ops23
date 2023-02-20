# -*- mode: ruby -*-
# vi: set ft=ruby :

# Since the webserver needs the IP of the DB server the two have to be started 
# in the right order and with storing the IP of the latter on the way:
#
# $ rm db_ip.txt | vagrant up | python store_ip.py

$ip_file = "db_ip.txt"

Vagrant.configure("2") do |config|
    config.vm.box = 'digital_ocean'
    config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
    config.ssh.private_key_path = '~/.ssh/id_rsa'
    config.vm.synced_folder ".", "/vagrant", type: "rsync"
  
    config.vm.define "dbserver", primary: true do |server|
      server.vm.provider :digital_ocean do |provider|
        provider.ssh_key_name = ENV["SSH_KEY_NAME"]
        provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
        provider.image = 'ubuntu-22-04-x64'
        provider.region = 'fra1'
        provider.size = 's-1vcpu-1gb'
        provider.privatenetworking = true
      end
  
      server.vm.hostname = "dbserver"

      server.trigger.after :up do |trigger|
        trigger.info =  "Writing dbserver's IP to file..."
        trigger.ruby do |env,machine|
          remote_ip = machine.instance_variable_get(:@communicator).instance_variable_get(:@connection_ssh_info)[:host]
          File.write($ip_file, remote_ip)
        end 
      end

      server.vm.provision "shell", inline: <<-SHELL
      
      
	sudo sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'

	wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | sudo apt-key add -

	sudo apt-get update

	sudo apt-get -y install postgresql
	
	sudo ufw allow ssh
        sudo ufw allow 5432
        sudo ufw enable
        
      SHELL
    end

    config.vm.define "webserver", primary: false do |server|
  
      server.vm.provider :digital_ocean do |provider|
        provider.ssh_key_name = ENV["SSH_KEY_NAME"]
        provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
        provider.image = 'ubuntu-22-04-x64'
        provider.region = 'fra1'
        provider.size = 's-1vcpu-1gb'
        provider.privatenetworking = true
      end

      server.vm.hostname = "webserver"

      server.trigger.before :up do |trigger|
        trigger.info =  "Waiting to create server until dbserver's IP is available."
        trigger.ruby do |env,machine|
          ip_file = "db_ip.txt"
          while !File.file?($ip_file) do
            sleep(1)
          end
          db_ip = File.read($ip_file).strip()
          puts "Now, I have it..."
          puts db_ip
        end 
      end

      server.trigger.after :provision do |trigger|
        trigger.ruby do |env,machine|
          File.delete($ip_file) if File.exists? $ip_file
        end 
      end

      server.vm.provision "shell", inline: <<-SHELL
        export DB_IP=`cat /vagrant/db_ip.txt`
        echo $DB_IP
	
	wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
	sudo dpkg -i packages-microsoft-prod.deb
	rm packages-microsoft-prod.deb
     	sudo apt-get update 
	sudo apt-get install -y dotnet-sdk-7.0
	sudo apt-get install -y aspnetcore-runtime-7.0
        
        cp -r /vagrant/* $HOME
        cd Server
        nohup dotnet run > out.log &
        sudo ufw allow ssh
        sudo ufw allow 5235
        sudo ufw enable
        
        echo "================================================================="
        echo "=                            DONE                               ="
        echo "================================================================="
        echo "Navigate in your browser to:"
        THIS_IP=`hostname -I | cut -d" " -f1`
        echo "http://${THIS_IP}:5235"
      SHELL
    end
    config.vm.provision "shell", privileged: false, inline: <<-SHELL
      sudo apt-get update
    SHELL

  end
