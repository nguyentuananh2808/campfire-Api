# AWS deployment notes (documentation only)

No AWS resources are provisioned by this repository. A pilot can use one EC2 instance running the Compose stack behind TLS, with PostgreSQL either on a private local volume with tested backup/restore or later on private RDS. RDS improves managed backups but adds recurring cost and network/secret configuration.

Restrict security groups to HTTPS and administrative access, keep the database non-public, inject secrets through an appropriate secret store, persist Data Protection keys, rotate logs and test restore. Budget alerts are notifications, not spending caps. Run reviewed migrations before the app; roll back the application only when schema compatibility permits it.
