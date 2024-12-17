describe('Login Form E2E Tests', () => {
    const apiUrl = Cypress.env('NEXT_PUBLIC_API_BASE_URL') || 'http://localhost:3000';
  
    const baseUrl = 'http://localhost:3000';

    beforeEach(() => {
      cy.visit(`${baseUrl}/log-in`);
    });
    
  
    it('should render the login form correctly', () => {
      cy.contains('Log in').should('exist');
      cy.get('input[name="email"]').should('exist');
      cy.get('input[name="password"]').should('exist');
      cy.get('button[type="submit"]').should('exist');
    });
  
    it('should show validation error for invalid email', () => {
      cy.get('input[name="email"]').type('invalid-email@c');
      cy.get('input[name="password"]').type('Password123!');
      cy.get('button[type="submit"]').click();
      cy.contains('E-mail is invalid.').should('be.visible');
    });
    it('should show validation error for weak password ', () => {
        cy.get('input[name="email"]').type('invalid-email@edu.pl');
        cy.get('input[name="password"]').type('password123');
        cy.get('button[type="submit"]').click();
        cy.contains('Password must have').should('be.visible');
    });

    it('should show server error on failed login', () => {
      cy.intercept('POST', `${apiUrl}/Account/login`, {
        statusCode: 401,
        body: { message: 'Invalid email or password' },
      });
  
      cy.get('input[name="email"]').type('test@example.com');
      cy.get('input[name="password"]').type('Wrongpassword1!');
      cy.get('button[type="submit"]').click();
  
      cy.contains('Login failed').should('be.visible');
    });
  
    it('should log in successfully and redirect to /play', () => {
      cy.intercept('POST', `${apiUrl}/Account/login`, {
        statusCode: 200,
        body: { email: 'test@example.com', username: 'testuser' },
      });
  
      cy.get('input[name="email"]').type('user@example2.com');
      cy.get('input[name="password"]').type('String1!');
      cy.get('button[type="submit"]').click();
  
      cy.url().should('include', '/play'); 
    });
  });
  