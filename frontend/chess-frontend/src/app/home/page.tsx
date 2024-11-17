import RootLayout from '../layout';
import Chessboard from '../components/chessBoard/chessBoard';
import { useDispatch } from 'react-redux';

export default function HomePage() {
import Chessboard from '../components/chessBoard';

export default function Page() {
  return (
    <RootLayout>
      <Chessboard/>
    </RootLayout>
  );
}
