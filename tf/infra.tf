terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 2.0"
    }
  }
}

provider "azurerm" {
  # The "feature" block is required for AzureRM provider 2.x. 
  # If you are using version 1.x, the "features" block is not allowed.
  # version = "~>2.0"
  features {}
  # Use Azure CLI to authencation
}

locals {
  group_name  = "DemoTf-${formatdate("MMDDHHmm", timestamp())}"
  location    = "eastasia"
  random_name = "__random__"
  # random_name             = random_string.rid.result
}

resource "random_string" "rid" {
  length  = 3
  special = false
  number  = false
}

resource "azurerm_resource_group" "demotf" {
  name     = local.group_name
  location = local.location

  tags = {
    environment = local.group_name
  }
}

resource "azurerm_app_service_plan" "demotf" {
  name                = "plan${local.random_name}"
  location            = azurerm_resource_group.demotf.location
  resource_group_name = azurerm_resource_group.demotf.name
  kind                = "Linux"
  reserved            = true

  sku {
    tier = "Standard"
    size = "S1"
  }

  tags = {
    environment = local.group_name
  }
}

resource "azurerm_app_service" "demotf" {
  name                = "web${local.random_name}"
  location            = azurerm_resource_group.demotf.location
  resource_group_name = azurerm_resource_group.demotf.name
  app_service_plan_id = azurerm_app_service_plan.demotf.id

  site_config {
    linux_fx_version = "DOTNETCORE|6.0"
  }

  app_settings = {
    "WEBSITE_TIME_ZONE" = "Asia/Taipei"
  }

  tags = {
    environment = local.group_name
  }
}
