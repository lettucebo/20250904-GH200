// Deploy cmd:
// az deployment group create --resource-group Demo0217 --template-file BICEP.bicep
// New-AzResourceGroupDeployment -ResourceGroupName Demo0217 -TemplateFile BICEP.bicep

@minLength(3)
@maxLength(24)
param defaultName string = 'demo0217'
param location string = 'eastasia'

@secure()
@description('This is demo description')
param password string

@description('The Windows version for the VM. This will pick a fully patched image of this given Windows version.')
@allowed([
  '2019-Datacenter'
  '2019-Datacenter-Core'
  '2019-Datacenter-Core-smalldisk'
  '2019-Datacenter-Core-with-Containers'
  '2019-Datacenter-Core-with-Containers-smalldisk'
  '2019-Datacenter-smalldisk'
  '2019-Datacenter-with-Containers'
  '2019-Datacenter-with-Containers-smalldisk'
])
param osVersion string = '2019-Datacenter'

resource demoVnet 'Microsoft.Network/virtualNetworks@2019-11-01' = {
  name: '${defaultName}vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
  }
}

resource demoSubnet 'Microsoft.Network/virtualNetworks/subnets@2021-02-01' = {
  parent: demoVnet
  name: '${defaultName}subnet'
  properties: {
    addressPrefix: '10.0.0.0/24'
  }
}

resource demoStor 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: '${defaultName}stor'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource demoPip 'Microsoft.Network/publicIPAddresses@2021-02-01' = {
  name: '${defaultName}pip'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    publicIPAllocationMethod: 'Dynamic'
    dnsSettings: {
      domainNameLabel: '${defaultName}vm'
    }
  }
}

resource demoNic 'Microsoft.Network/networkInterfaces@2021-02-01' = {
  name: '${defaultName}nic'
  location: location
  properties: {
    ipConfigurations: [
      {
        name: 'ipconfig1'
        properties: {
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: {
            id: demoPip.id
          }
          subnet: {
            id: demoSubnet.id
          }
        }
      }
    ]
  }
}

resource demoVm 'Microsoft.Compute/virtualMachines@2020-12-01' = {
  name: '${defaultName}vm'
  location: location
  properties: {
    hardwareProfile: {
      vmSize: 'Standard_A2_v2'
    }
    osProfile: {
      computerName: '${defaultName}vm'
      adminUsername: 'demouser'
      adminPassword: password
    }
    storageProfile: {
      imageReference: {
        publisher: 'MicrosoftWindowsServer'
        offer: 'WindowsServer'
        sku: osVersion
        version: 'latest'
      }
      osDisk: {
        name: 'name'
        caching: 'ReadWrite'
        createOption: 'FromImage'
      }
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: demoNic.id
        }
      ]
    }
    diagnosticsProfile: {
      bootDiagnostics: {
        enabled: true
        storageUri: demoStor.properties.primaryEndpoints.blob
      }
    }
  }
}
