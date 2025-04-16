variable "defaultRegion" {
  default = "us-east-1"
}

variable "brokerpassword" {
  description = "Secret passed from GitHub Actions"
  type        = string
}

variable "awsAccessKeyId" {
  description = "Secret passed from GitHub Actions"
  type        = string
}

variable "awsSecretAccessKey" {
  description = "Secret passed from GitHub Actions"
  type        = string
}

variable "awsSessionToken" {
  description = "Secret passed from GitHub Actions"
  type        = string
}