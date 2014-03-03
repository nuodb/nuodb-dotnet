param($installPath, $toolsPath, $package, $project)                                                                                                                    
Add-EFProvider $project 'NuoDb.Data.Client' 'EntityFramework.NuoDb.NuoDbProviderServices, EntityFramework.NuoDb'
Add-EFDefaultConnectionFactory $project 'EntityFramework.NuoDb.NuoDbConnectionFactory, EntityFramework.NuoDb'