import RootLayout from '../layout';
import Chessboard from '../components/chessBoard/chessBoard';
import { useDispatch } from 'react-redux';

export default function HomePage() {
  return (
    <RootLayout>
      <Chessboard/>
    </RootLayout>
  );
}
