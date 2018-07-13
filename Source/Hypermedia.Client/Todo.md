#Bugs

## Not found entity
If a entity is not pressent in the Siren document is should set a (single) property to `null` instead of throwing an exception. Also add Mandatory attribute so this exception can be configured.