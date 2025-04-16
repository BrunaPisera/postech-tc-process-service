terraform {
  backend "s3" {
    bucket = "tc-tf-backend"
    key    = "backend/terraform_process_service.tfstate"
    region = "us-east-1"
  }
}