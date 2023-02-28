Vagrant.configure("2") do |config|
  config.vm.box = "ubuntu/focal64"

  # Use DigitalOcean provider
  config.vm.provider :digital_ocean do |provider|
    provider.token = ENV['DIGITAL_OCEAN_TOKEN']
    provider.ssh_key_name = ENV['SSH_KEY_NAME']
    provider.image = 'ubuntu-22-04-x64'
    provider.region = 'fra1'
    provider.size = 's-1vcpu-1gb'
  end


  # Set hostname
  config.vm.hostname = "minitwit-prod"


  # Set up SSH key
  config.ssh.private_key_path = "~/.ssh/id_rsa"

  # Provision the machine
  config.vm.provision "shell", inline: <<-SHELL
    # Update packages
    sudo apt-get update

    # Install Docker
    sudo apt-get install -y docker.io

    # Add current user to the docker group
    sudo usermod -aG docker $(whoami)

    # Start Docker on boot
    sudo systemctl enable docker
    
    # Enable firewall and open port
    sudo ufw allow ssh
    sudo ufw allow 5235
    sudo ufw enable
    sudo ufw reload
  SHELL

  # Pull Docker image from Docker Hub and run it
  config.vm.provision "docker" do |d|

    d.pull_images "bhviid/devops:0.0.1.1"
    d.run "web", args: "-p 5235:80", image: "bhviid/devops:0.0.1.1"

  end
end
