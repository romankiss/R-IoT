# AWS PostgreSQL Database Creation & Grafana Connection
***WARNING: Vibe-Coded material - no guarantee included***
## 1. Create PostgreSQL Database on AWS

### Using Amazon RDS

1. **Login to AWS Console**  
    Go to [AWS Management Console](https://console.aws.amazon.com/).

2. **Navigate to RDS**  
    Search for "RDS" and open the service.

3. **Create Database**  
    - Click **Create database**.
    - Select **Standard Create**.
    - Choose **PostgreSQL** as the engine.
    - Select the version and instance type.
    - Set DB instance identifier, master username, and password.
    - Configure storage and connectivity (VPC, subnet, security group).
    - Enable public access if Grafana is external.
    - Click **Create database**.

4. **Get Connection Details**  
    - After creation, note the **endpoint**, **port**, **username**, and **database name**.

## 2. Connect Grafana to AWS PostgreSQL

### Prerequisites

- Grafana installed (local or cloud).
- PostgreSQL endpoint accessible from Grafana.

### Steps

1. **Login to Grafana**
2. **Add Data Source**
    - Go to **Configuration > Data Sources**.
    - Click **Add data source**.
    - Select **PostgreSQL**.

3. **Configure Connection**
    - **Host**: `<RDS endpoint>:<port>`
    - **Database**: `<database name>`
    - **User**: `<master username>`
    - **Password**: `<master password>`
    - **SSL Mode**: Set as required (often `require` for AWS).

4. **Save & Test**
    - Click **Save & Test** to verify connection.

## 3. Troubleshooting

- Ensure security group allows inbound traffic on PostgreSQL port (default: 5432).
- Check VPC/subnet settings for connectivity.
- Use SSL certificates if required.

## References

- [AWS RDS Documentation](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/CHAP_PostgreSQL.html)
- [Grafana PostgreSQL Data Source](https://grafana.com/docs/grafana/latest/datasources/postgres/)