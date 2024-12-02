# DeskMotion Testing Backlog

## Planned Improvements

### 1. Response Type Standardization
- [ ] Create `ApiResponse<T>` generic class for standardized responses
- [ ] Update all controller endpoints to use `ApiResponse<T>`
- [ ] Modify test assertions to handle new response type
- [ ] Add response type documentation

### 2. Test Coverage
- [ ] Complete unit tests for DeskService
- [ ] Add integration tests for desk movement operations
- [ ] Implement error handling tests
- [ ] Add preset management tests

### 3. Test Infrastructure
- [ ] Improve test base class with more helper methods
- [ ] Add test data factories
- [ ] Implement test cleanup methods
- [ ] Add performance benchmarks

### 4. Edge Cases
- [ ] Test concurrent desk operations
- [ ] Test network timeout scenarios
- [ ] Test invalid height values
- [ ] Test preset conflicts

## Current Test Status
- Basic desk operations tests are working
- Using Dictionary-based responses temporarily
- FluentAssertions for improved test readability
- Mock setup helpers in base class

## Known Issues
1. Response type inconsistency across endpoints
2. Limited error scenario coverage
3. Missing integration tests
4. Incomplete preset feature tests

## Next Steps When Resuming
1. Implement `ApiResponse<T>` class
2. Update one endpoint at a time to use new response type
3. Update corresponding tests
4. Add more comprehensive error handling tests
