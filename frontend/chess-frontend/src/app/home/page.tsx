'use client';
import RootLayout from '../layout';
import Chessboard from '../components/chessBoard/chessBoard';

export default function HomePage() {

  return (
      <Chessboard 
      isDraggablePiece={() => true}/>
  );
}
