import RootLayout from "../layout";
import Chessboard from "../components/chessBoard/chessBoard";
import GameModeModal from "../components/gameModeModal/gameModeModal";

export default function Page() {
  return (
    <RootLayout>
      <div style={containerStyles}>
        <div style={chessboardContainerStyles}>
          <Chessboard />
        </div>

        <div style={modalContainerStyles}>
          <GameModeModal/>
        </div>
      </div>
    </RootLayout>
  );
}

const containerStyles = {
  display: 'flex',
  justifyContent: 'flex-end', 
  alignItems: 'flex-start', 
  padding: '20px',
  gap: '30px',
};

const chessboardContainerStyles = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
};

const modalContainerStyles = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  height: '600px', // Wysokość kontenera modala, np. równa szachownicy
  width: '400px',  // Szerokość kontenera modala
  borderRadius: '15px',
  backgroundColor: 'rgba(255, 255, 255, 0.1)', // Transparentne tło dla efektu glassmorphism
  boxShadow: '0 4px 15px rgba(0, 0, 0, 0.3)', // Miękki cień
  backdropFilter: 'blur(10px)', // Rozmycie tła
};
