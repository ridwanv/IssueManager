# API Specification

## REST API Specification

```yaml
openapi: 3.0.0
info:
  title: Issue Management System API
  version: 1.0.0
  description: RESTful API for WhatsApp-enabled Issue Management System with Clean Architecture and CQRS patterns
  contact:
    name: API Support
    email: support@issuemanager.com
servers:
  - url: https://api.issuemanager.com/v1
    description: Production API Server
  - url: https://staging-api.issuemanager.com/v1
    description: Staging API Server

security:
  - BearerAuth: []

paths:
  # Issue Management Endpoints
  /issues:
    get:
      summary: Get paginated list of issues
      tags: [Issues]
      parameters:
        - name: page
          in: query
          schema:
            type: integer
            default: 1
        - name: pageSize
          in: query
          schema:
            type: integer
            default: 20
        - name: status
          in: query
          schema:
            $ref: '#/components/schemas/IssueStatus'
        - name: priority
          in: query
          schema:
            $ref: '#/components/schemas/IssuePriority'
        - name: assignedUserId
          in: query
          schema:
            type: string
            format: uuid
      responses:
        '200':
          description: Paginated list of issues
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PaginatedIssueResponse'
    post:
      summary: Create new issue
      tags: [Issues]
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CreateIssueCommand'
      responses:
        '201':
          description: Issue created successfully
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/IssueResponse'

  /issues/{id}:
    get:
      summary: Get issue by ID
      tags: [Issues]
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      responses:
        '200':
          description: Issue details
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/IssueResponse'
        '404':
          $ref: '#/components/responses/NotFound'
    
    put:
      summary: Update issue
      tags: [Issues]
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/UpdateIssueCommand'
      responses:
        '200':
          description: Issue updated successfully
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/IssueResponse'

components:
  securitySchemes:
    BearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT

  schemas:
    # Core Domain Models
    Issue:
      type: object
      properties:
        id:
          type: string
          format: uuid
        title:
          type: string
          maxLength: 200
        description:
          type: string
        category:
          $ref: '#/components/schemas/IssueCategory'
        priority:
          $ref: '#/components/schemas/IssuePriority'
        status:
          $ref: '#/components/schemas/IssueStatus'
        reporterContactId:
          type: string
          format: uuid
        assignedUserId:
          type: string
          format: uuid
          nullable: true
        productId:
          type: string
          format: uuid
          nullable: true
        createdAt:
          type: string
          format: date-time
        updatedAt:
          type: string
          format: date-time
        tenantId:
          type: string
          format: uuid

    # Enums
    IssueCategory:
      type: string
      enum: [technical, billing, general, feature]

    IssuePriority:
      type: string
      enum: [low, medium, high, critical]

    IssueStatus:
      type: string
      enum: [new, in_progress, resolved, closed]

    # Command Models
    CreateIssueCommand:
      type: object
      required: [title, description, reporterContactId, category, priority]
      properties:
        title:
          type: string
          maxLength: 200
        description:
          type: string
        reporterContactId:
          type: string
          format: uuid
        category:
          $ref: '#/components/schemas/IssueCategory'
        priority:
          $ref: '#/components/schemas/IssuePriority'
        productId:
          type: string
          format: uuid
          nullable: true

  responses:
    NotFound:
      description: Resource not found
      content:
        application/json:
          schema:
            type: object
            properties:
              error:
                type: object
                properties:
                  code:
                    type: string
                  message:
                    type: string
```
