terraform {
  required_providers {
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "~> 2.0"
    }
  }
}


# Set the variable value in *.tfvars file
# or using -var="do_token=..." CLI option
variable "do_token" {}
variable "pub_key" {}
variable "pvt_key" {}

provider "digitalocean" { token = var.do_token }

resource "digitalocean_ssh_key" "minitwit" {
  name       = "tf"
  public_key = file(var.pub_key)
}

resource "digitalocean_tag" "taerteform" {
  name = "taerteform"
}

# Configure the DigitalOcean Provider
# provider "digitalocean" {
#   token = var.do_token
# }

# Front end
# resource "digitalocean_droplet" "minitwit" {
#   # ...
# }

# # Utility server (Grafana, Prometheus, Postgres, Elastic service, Kibana, )
# resource "digitalocean_droplet" "gigatwit" {
#   # ...
# }

# Simulation API server Swarm manager
resource "digitalocean_droplet" "slimtwit-swarm-manager" {
  name   = "sim-swarm-manager"
  image  = "docker-18-04"
  region = "fra1"
  size   = "s-1vcpu-1gb"
  tags   = [digitalocean_tag.taerteform.id]

  connection {
    user        = "root"
    host        = self.ipv4_address
    type        = "ssh"
    private_key = file(var.pvt_key)
    timeout     = "2m"
  }

  # what this?
  ssh_keys = [digitalocean_ssh_key.minitwit.fingerprint]

  provisioner "file" {
    source      = "../Docker/prod-slimtwit-compose/docker-compose.yml"
    destination = "~/docker-compose.yml"
  }

  provisioner "remote-exec" {
    inline = [
      "ufw allow 22/tcp",
      "ufw allow 2376/tcp",
      "ufw allow 2377/tcp",
      "ufw allow 7946/tcp",
      "ufw allow 7946/udp",
      "ufw allow 4789/udp",
      "ufw reload",
      "cd ~/",
      "docker stack deploy -c docker-compose.yml slimi",

      #init unser swarm
      "docker swarm init --advertise-addr ${self.ipv4_address}"
    ]
  }



  # GET THE WORKER JOIN TOKEN FROM THE NEWLY CREATED SWARM MANAGER NODE
  provisioner "local-exec" {
    command = <<EOS
      ssh -o 'StrictHostKeyChecking=no' root@${self.ipv4_address} -i ssh_key/terraform
      'docker swarm join-token worker -q' > temp/worker_token
    EOS
  }
}

# Simulation API server Swarm nodes
resource "digitalocean_droplet" "slimtwit" {
  depends_on = [digitalocean_droplet.slimtwit-swarm-manager]
  image      = "docker-18-04"
  count      = 2
  name       = "slimtwit-node${count.index + 1}"
  size       = "s-1vcpu-1gb"
  tags       = [digitalocean_tag.taerteform.id]
  connection {
    user        = "root"
    host        = self.ipv4_address
    type        = "ssh"
    private_key = file(var.pvt_key)
    timeout     = "2m"
  }

  ssh_keys = [digitalocean_ssh_key.minitwit.fingerprint]

  provisioner "file" {
    source      = "temp/worker_token"
    destination = "/root/worker_token"
  }

  provisioner "remote-exec" {
    inline = [
      "ufw allow 22/tcp",
      "ufw allow 2376/tcp",
      "ufw allow 2377/tcp",
      "ufw allow 7946/tcp",
      "ufw allow 7946/udp",
      "ufw allow 4789/udp",
      "ufw reload",
      #join swarm
      "docker swarm join $(cat worker_token) ${digitalocean_droplet.slimtwit-swarm-manager.ipv4_address}"
    ]
  }
}
