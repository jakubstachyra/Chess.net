describe("Game Mode Modal", () => {
    it("should load the game mode modal and navigate to play-with-computer/1", () => {

      cy.visit("http://localhost:3000/play");
  
      cy.contains("Select Game Mode").should("be.visible");
  
      cy.contains("🤖 Play vs Computer").click();
  
      cy.contains("Select Timer").should("be.visible");
  
      cy.get("select").select("5 min");
  

      cy.contains("Play").click();

    });
  });
  
  
  describe("Chessboard Computer Gameplay", () => {
    it("should load the chessboard and allow making moves", () => {

      cy.visit("http://localhost:3000/play-with-computer/1");

      cy.get(".chessboard").should("be.visible");
  

      cy.get('[data-square="e2"]').click(); 
      cy.get('[data-square="e4"]').click(); 

      cy.get(".chessboard").should("exist");
  
      cy.on("window:alert", (str) => {
        expect(str).to.include("Error");
      });
    });
  });
  