# Task: Scaffold Order Microservice

## Objective
Create a complete microservice solution for managing orders, following the IaResearch architecture patterns and rules.

## Requirements

### Solution Structure
- **Solution Name**: `IaResearch.OrderService`
- **Feature**: `Order`
- **Architecture**: Use the IaResearch MCP tool to generate all structure and code

### Components Required
1. **Order** - Domain model representing an order
2. **CreateOrderCommand** - Command to create a new order
3. **CreateOrderCommandHandler** - Handler to process the CreateOrderCommand
4. **IOrderRepository** - Repository interface for order persistence
5. **OrderRepository** - Repository implementation for order persistence

## Constraints
- All code MUST be generated using the IaResearch Architecture MCP tool
- Do NOT manually create files, infer file paths, or guess layer assignments
- Apply the MCP tool's output exactly as returned - no modifications
- Ensure the `SolutionName` and `Feature` parameters are consistent across all MCP calls

## Expected Outcome
A complete solution with:
- Solution file (.sln)
- Project structure organized by layers (Domain, Application, Infrastructure)
- All required component files with proper namespaces, using statements, and architecture compliance
- No additional files beyond what the MCP tool generates