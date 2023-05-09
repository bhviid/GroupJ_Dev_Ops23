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
variable "postgres_user_id" {}
variable "postgres_password" {}
variable "postgres_db" {}

provider "digitalocean" { token = var.do_token }

resource "digitalocean_ssh_key" "minitwit" {
  name       = "tf"
  public_key = file(var.pub_key)
}

resource "digitalocean_tag" "taerteform" {
  name = "taerteform"
}
######################################################################################################################


################################################ Front end ##########################################################
resource "digitalocean_droplet" "minitwit" {
  depends_on = [ digitalocean_droplet.gigatwit-database ]
  name   = "minitwit-frontend"
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

  # publci key
  ssh_keys = [digitalocean_ssh_key.minitwit.fingerprint]
  # ...

  provisioner "file" {
    source      = "../Docker/prod-minitwit-compose/docker-compose.yml"
    destination = "./docker/docker-compose.yml"
  }

  provisioner "remote-exec" {
    inline = [
      "mv ./docker ./docker-compose.yml",
      "export connection_string=\"Server=${digitalocean_droplet.gigatwit-database.ipv4_address};Port=5432;Database=${var.postgres_db};User Id=${var.postgres_user_id};Password=${var.postgres_password}\"",
      "source ~/.bashrc",

      "docker-compose -f docker-compose.yml up -d"
    ]
  }
}
##################################################################################################################

############################################ Database droplet ####################################################
resource "digitalocean_droplet" "gigatwit-database" {
  name   = "gigatwit-db"
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

  # publci key
  ssh_keys = [digitalocean_ssh_key.minitwit.fingerprint]

  provisioner "file" {
    source      = "../Docker/prod-database-compose/docker-compose.yml"
    destination = "./docker/docker-compose.yml"
  }

  provisioner "remote-exec" {
    inline = [
      "mv ./docker ./docker-compose.yml",
      "export POSTGRES_USER=${var.postgres_user_id}",
      "export POSTGRES_PASSWORD=${var.postgres_password}",
      "export POSTGRES_DB=${var.postgres_db}",
      "source ~/.bashrc",

      "docker-compose -f docker-compose.yml up -d"
    ]
  }


}
######################################################################################################################


################################################# Utility server ###################################################
##################################(Grafana, Prometheus, Postgres, Elastic service, Kibana, )#########################
resource "digitalocean_droplet" "gigatwit-utility" {
  depends_on = [digitalocean_droplet.gigatwit-database, digitalocean_droplet.slimtwit-swarm-manager]
  name       = "gigatwit-utility"
  image      = "docker-18-04"
  region     = "fra1"
  size       = "s-1vcpu-1gb"
  tags       = [digitalocean_tag.taerteform.id]

  connection {
    user        = "root"
    host        = self.ipv4_address
    type        = "ssh"
    private_key = file(var.pvt_key)
    timeout     = "20m"
  }

  # publci key
  ssh_keys = [digitalocean_ssh_key.minitwit.fingerprint]

  provisioner "file" {
    source      = "../Docker/prod-utility-compose/docker-compose.yml"
    destination = "./docker/docker-compose.yml"
  }

  provisioner "file" {
    source      = "../Docker/config/utility-prometheus.yml"
    destination = "utility-prometheus.yml"
  }
  #prometheus_sim_ip
  #prometheus_minitwit_ip
  provisioner "remote-exec" {
    inline = [
      "mv ./docker ./docker-compose.yml",
      ## INSERT IP FOR PROMETHEUS SIM API - prometheus_sim_ip
      "sed -i -e 's/prometheus_sim_ip/${digitalocean_droplet.slimtwit-swarm-manager.ipv4_address}/g' utility-prometheus.yml",
      ## INSERT IP FOR NETDATA for database - prometheus_database_ip
      "sed -i -e 's/prometheus_database_ip/${digitalocean_droplet.gigatwit-database.ipv4_address}/g' utility-prometheus.yml",
      ## INSERT IP FOR MINTWIT 
      #"sed -i -e 's/old-text/new-text/g' utility-prometheus.yml",

      "docker-compose -f docker-compose.yml up -d"
    ]
  }

}
######################################################################################################################

###################################### Simulation API server Swarm manager #######################################
resource "digitalocean_droplet" "slimtwit-swarm-manager" {
  depends_on = [digitalocean_droplet.gigatwit-database, digitalocean_droplet.minitwit]
  name       = "sim-swarm-manager"
  image      = "docker-18-04"
  region     = "fra1"
  size       = "s-1vcpu-1gb"
  tags       = [digitalocean_tag.taerteform.id]

  connection {
    user        = "root"
    host        = self.ipv4_address
    type        = "ssh"
    private_key = file(var.pvt_key)
    timeout     = "2m"
  }

  # what this?
  ssh_keys = [digitalocean_ssh_key.minitwit.fingerprint]

  #Compose file for the stack
  provisioner "file" {
    source      = "../Docker/prod-slimtwit-compose/docker-compose.yml"
    destination = "./docker/docker-compose.yml"
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

      #init unser swarm
      "docker swarm init --advertise-addr ${self.ipv4_address}",

      #DO NOT REMOVE THIS LINE - OTHERWISE YOU WILL HAVE A BAD TIME WITH WEIRD PATHS.
      "mv ./docker ./docker-compose.yml",
      "export connection_string=\"Server=${digitalocean_droplet.gigatwit-database.ipv4_address};Port=5432;Database=${var.postgres_db};User Id=${var.postgres_user_id};Password=${var.postgres_password}\"",
      "source ~/.bashrc",
      "docker stack deploy -c docker-compose.yml slimi",
    ]
  }

  # GET THE WORKER JOIN TOKEN FROM THE NEWLY CREATED SWARM MANAGER NODE
  provisioner "local-exec" {
    command = <<EOS
        ssh -o StrictHostKeyChecking=no root@${self.ipv4_address} -i ${var.pvt_key} docker swarm join-token worker -q > temp/worker_token
      EOS
  }
}
######################################################################################################################

##################################### Simulation API server Swarm nodes ##############################################
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
    destination = "worker_token"
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
      "docker swarm join --token $(cat worker_token) ${digitalocean_droplet.slimtwit-swarm-manager.ipv4_address}:2377"
    ]
  }
}
######################################################################################################################
