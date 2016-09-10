# SlayingDragonsES
.NET Implementation and demonstration of Elasticsearch

## Amazon Web Services
Create an AWS user, set up billing, sign in. There are two essential steps:
1) Creating the IAM User
2) Creating Elasticsearch Managed Service

### Creating IAM User
- Click on your name (top right) --> Security Credentials
- Users tab --> Create New User
- Enter name(s), check the box to generate an access key for each user.
- Each user will have security credentials with API Key ID and Secret Key. These will be needed for later.
  - **Note**: Additional keys can be generated later, but it may be a good idea to download them.

### Elasticsearch Managed Service
- On the landing page, navigate to Analytics --> Elasticsearch Service
- Click _Create a new domain_
- Enter a domain name and click _Next_
  - This project uses Elasticsearch version 2.3
- The next page is the configuration page and where a lot of the fun happens. Scaling, replication and additional advanced settings can be set here. Settings for this demo were as follows:
  - **Instance Count**: 1
  - **Instance Type**: t2.micro.elasticsearch
  - **Storage Type**: EBS
  - **EBS Volume Type**: General Purpose (SSD)
  - **EBS Volume Size**: 10
- Move on to the next page where you can set up your IAM policy. I selected *Allow Access to the domain from Specific IP(s)* with a number of known IP addresses where I go and my VPN. You can choose whatever you would like.
- Clicking _Next_ moves on to a review page, _Confirm and Create_ will create the managed service.
- Once created, you can navigate to the Elasticsearch Service and retrieve the hosted URL, which should inserted into the ```aws.config.json``` file.
- You're ready for the next steps!

### Entity Framework Database
Out of the box, the default sql server instance is LocalDB. If LocalDB is not installed, it can be installed by following the links and instructions from [this](https://msdn.microsoft.com/en-us/library/hh510202.aspx) MSDN article. Additional changes to the server, including testing with a deployed server will require modification of *web.config*

### Building and running this solution
- Clone this repository
- Open in Visual Studio (built with 2015, should work with 2013)
- Build the solution
- Open package manager console
- Run ```Update-Database```
- Create a file in the ./Elasticsearch/Elasticsearch Directory called ```aws.config.json``` with the following structure:
```json
{
  "Key": "<aws_credential_key>",
  "Secret": "<aws_credential_secret>",
  "Region": "<aws_region>",
  "Uri": "<aws_elasticsearch_url>"
}
```
