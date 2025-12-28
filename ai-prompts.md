# AI Prompts

This is a high level overview of what prompts were used to create the project.

## Project Scaffolding
### Plan Phase 
__Prompt__: ```Create a HelloWorld monorepo configuration having a C# backend with .NET 10 and an Angular frontend. Also add Docker support for local development.```

__Planning Agent Response__: ```Further Considerations
1. Port configuration: Backend on 5000 (external) mapping to 8080 (internal .NET 10 default), frontend on 4200 - need other port mappings?
2. Development containers: Include full VS Code devcontainer setup with extensions, or prefer manual Docker Compose workflow?
3. Database integration: Plan for future database service in Docker Compose, or keep simple for HelloWorld scope?```

__Additional Prompt Instructions__:
1. The default port mappings are sufficient.
2. Use a Docker Compose workflow.
3. Keep it simple for the HelloWorld scope.


## Project Main Features

### Open AI Vision API for OCR
__Prompt__: ```Create a plan for integrating the Open AI Vision API into the backend.```

__Planning Agent Response__: ```Further Considerations
1. Structured data format - Should extracted data follow a specific schema (vendor, date, items array, totals) or flexible key-value pairs?
2. OCR accuracy handling - How to handle partial extractions, unclear text, or multiple interpretation possibilities?
3. Image preprocessing - Auto-rotate, enhance contrast, or resize images before sending to OpenAI for better OCR results?```

__Additional Prompt Instructions__:
1. Use flexible key-value pairs for extracted data.
2. Include all extracted data, even partial or unclear. In the case of multiple interpretation possibilities, choose the highest probable option.
3. Image preprocessing is not needed.