# TaskManager

A full-stack management App built with ASP.NET Core and React, AWS deployment.

# publish .NET Project

dotnet publish TaskManager.API -c Release

AWS Mac/Linux:

```bash
aws cloudformation deploy \
  --template-file cloudformation.yaml \
  --stack-name TaskManager-Stack \
  --parameter-overrides \
    Environment=dev \
    DatabaseUsername=postgres \
    DatabasePassword=123456 \

```

AWS Windows:

```bash
aws cloudformation deploy `
  --template-file cloudformation.yaml `
  --stack-name TaskManager-Stack `
  --parameter-overrides `
    Environment=dev `
    DatabaseUsername=postgres `
    DatabasePassword=123456 `
    JwtSecret=pz8CR2vH6jXm9qLs5f3TdAuKbPgE7NwY4aVQxZtBD1iG0ySMoF `
    AdminEmail=szz185@gmail.com `
  --capabilities CAPABILITY_IAM

```

#SAM Cli

```bash
sam deploy --template-file cloudformation.yaml --stack-name TaskManager-Stack --parameter-overrides Environment=dev DatabaseUsername=postgres DatabasePassword=123456 JwtSecret=pz8CR2vH6jXm9qLs5f3TdAuKbPgE7NwY4aVQxZtBD1iG0ySMoF AdminEmail=szz185@gmail.com --capabilities CAPABILITY_IAM
```

### update S3
```bash
npm run build
aws s3 sync out/ s3://task-manager-front185 --delete
```