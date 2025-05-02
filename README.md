# Task Management System

A fully-featured task management application built with modern full-stack architecture and AWS cloud services integration.

## Project Overview

Task Management System is a web application that allows users to create, manage, and track personal tasks. This project implements a decoupled architecture with separate frontend and backend components, combined with AWS cloud services for secure user authentication and data storage.

## Technology Stack

### Frontend Technologies
- **Next.js**: React framework for building high-performance single-page applications
- **Redux Toolkit**: State management for handling complex application state and async logic
- **AWS Amplify**: Integration with AWS Cognito for user authentication services
- **TailwindCSS**: Implementation of responsive design and modern UI interfaces
- **TypeScript**: Ensuring type safety and code maintainability

### Backend Technologies
- **ASP.NET Core 8**: Building RESTful APIs and business logic implementation
- **Entity Framework Core**: ORM framework for simplified database operations
- **C# 12**: Leveraging the latest language features for improved development efficiency
- **PostgreSQL**: Relational database for storing application data

### AWS Cloud Services Integration
- **Amazon Cognito**: User identity authentication and authorization
- **AWS Lambda**: Backend service deployment with serverless architecture
- **API Gateway**: Managing API requests and responses
- **JWT**: Token-based secure communication

## Core Features

1. **Authentication System**:
    - Secure login/registration flow based on AWS Cognito
    - JWT token authentication
    - Automatic token refresh mechanism

2. **Task Management**:
    - Create, Read, Update, Delete (CRUD) tasks
    - Task priority settings
    - Due date management
    - Task completion status tracking

3. **User Experience**:
    - Responsive design for mobile and desktop
    - Page updates without refresh
    - State persistence

## Architecture Highlights

### Frontend Architecture
- **Redux State Management**: Implementing global state consistency, ensuring correct data refresh when users switch
- **React Component Design**: Component reusability and state isolation
- **TypeScript Type System**: Ensuring code quality and maintainability

### Backend Architecture
- **Layered Architecture**: Clear separation of controllers, service layer, and data access layer
- **Dependency Injection**: Decoupling system components for easier testing and maintenance
- **Lambda Serverless Deployment**: Elastic scaling with pay-as-you-go model

### Security Implementation
- **Cognito User Pools**: Managing user identities and security
- **JWT Validation**: Preventing unauthorized access
- **CORS Policies**: Securing the API
- **JWKS Integration**: Implementing AWS public key authentication

## Deployment Strategy

The project employs modern DevOps procedures using AWS cloud-native services:

1. **Frontend Deployment**:
    - AWS Amplify hosting for static resources
    - CloudFront CDN for global access acceleration

2. **Backend Deployment**:
    - AWS Lambda serverless deployment
    - API Gateway for request routing
    - PostgreSQL RDS database service

## Future Plans

- Add task sharing functionality
- Implement team collaboration spaces
- Integrate calendar and reminder features
- Develop mobile applications

---

*This project demonstrates my full-stack development capabilities, including modern frontend frameworks, .NET backend development, and AWS cloud services integration, particularly in user authentication, serverless architecture, and responsive design.*