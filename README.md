# Azure.Functions.AlertsToStride
Send azure availability alerts to stride

0. Deploy the azure function
1. Create a stride application 
[https://developer.atlassian.com/apps/](https://developer.atlassian.com/apps/)
2. Add the application to the rooms to push alerts to
3. Set the Azure function app settings
```
    "Stride:API": "https://api.atlassian.com/site/",
    "Stride:CloudId": "00023eb1-9acf-2239-9bb1-98efa553f098",   //Your stride cloud id
    "Stride:ClientId": "Chbg4",   //Your app client id
    "Stride:ClientSecret": "-v66exDjuU7N"    //Your app client secret
```
4. Open application insight availability test
5. Edit test alerts, add following webhooks:
```

```
