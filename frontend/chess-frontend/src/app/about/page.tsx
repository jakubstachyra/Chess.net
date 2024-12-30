import BackgroundUI from "app/components/backgroundUI/pages";

export default function AboutPage() {
    return (
      <div style = {backgroundContainerStyles}>
        <BackgroundUI>
          <h1>About</h1>
          <h2>Siema</h2>
      </BackgroundUI>
      </div>
      
    );
}

const backgroundContainerStyles = {
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    width: "50%", 
    height: "70%", 
    margin: "auto", 
    borderRadius: "15px",
  };