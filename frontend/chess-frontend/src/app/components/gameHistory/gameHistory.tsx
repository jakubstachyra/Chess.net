import React, { useState, useEffect, useRef } from "react";
import axios from "axios";
import BackgroundUI from "../backgroundUI/pages";
import { useSelector } from "react-redux";
import ChessboardComponent from "../chessBoard/chessBoard";
import { useRouter } from "next/navigation";
import "./gameHistory.css";

const GameHistory = () => {
  const [games, setGames] = useState([]);
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [shouldFetchNext, setShouldFetchNext] = useState(false);
  const [offset, setOffset] = useState(0);
  const [hasMore, setHasMore] = useState(true);

  // Ekstrakcja danych użytkownika z Reduxa
  const reduxUser = useSelector((state) => state.user);
  const currentUser = reduxUser.user;
  const userId = currentUser ? currentUser.userID : null;
  const username = currentUser ? currentUser.username : '';

  const limit = 3;
  const listContainerRef = useRef(null);
  const counterRef = useRef(0);
  const router = useRouter();

  useEffect(() => {
    if (userId && counterRef.current === 0) {
      fetchGames(userId);
      counterRef.current++;
    }
  }, [userId]);

  useEffect(() => {
    if (userId && shouldFetchNext) {
      fetchMoreGames(userId);
    }
  }, [shouldFetchNext, userId]);
      
  const handleGameClick = (gameId) => {
    router.push(`/history/game-details?gameId=${gameId}`);
  };

  const fetchGames = async (playerId) => {
    try {
      setInitialLoading(true);
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
      const firstLimit = 6;
      console.log("pytam");
      const response = await axios.get(
        `${API_BASE_URL}/games/${playerId}?limit=${firstLimit}&offset=0&detailed=false`
      );

      if (response.data.length < firstLimit) {
        setHasMore(false);
      }
      const mappedGames = response.data.map((game) => ({
        gameId: game.gameId,
        lastFen: game.lastFen,
        result: game.result,
        whitePlayer: game.whitePlayer,
        blackPlayer: game.blackPlayer,
      }));

      setOffset((prevOffset) => prevOffset + firstLimit);
      setGames((prevGames) => [...prevGames, ...mappedGames]);
    } catch (error) {
      console.error("Error fetching game history:", error);
    } finally {
      setInitialLoading(false);
    }
  };

  const fetchMoreGames = async (playerId) => {
    try {
      setLoading(true);
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
      const response = await axios.get(
        `${API_BASE_URL}/games/${playerId}?limit=${limit}&offset=${offset}&detailed=false`
      );

      if (response.data.length < limit) {
        setHasMore(false);
      }

      const mappedGames = response.data.map((game) => ({
        id: game.id || game.gameId,
        lastFen: game.lastFen,
        result: game.result,
      }));

      if (response.data.length > 0) {
        setGames((prevGames) => [...prevGames, ...mappedGames]);
        setOffset((prevOffset) => prevOffset + limit);
      }
    } catch (error) {
      console.error("Error fetching game history:", error);
    } finally {
      setLoading(false);
      setShouldFetchNext(false);
    }
  };

  const handleScroll = () => {
    if (!listContainerRef.current || !hasMore) return;

    const { scrollTop, scrollHeight, clientHeight } = listContainerRef.current;
    if (scrollTop + clientHeight >= scrollHeight - 5) {
      if (!shouldFetchNext && !loading) {
        setShouldFetchNext(true);
      }
    }
  };

  const displayedGames = hasMore
    ? [...games, ...Array(limit - (games.length % limit)).fill(null)]
    : games;

  return (
    <BackgroundUI>
      <h1 style={{ color: "white", fontSize: "18px", marginBottom: "5px" }}>Game history</h1>
      {initialLoading ? (
        <div className="loading-spinner bounce-spinner">
          <div className="spinner"></div>
        </div>
      ) : (
        <div className="game-history">
          <div
            className={`games-list-container ${!hasMore ? "no-scroll" : ""}`}
            ref={listContainerRef}
            onScroll={handleScroll}
          >
            <div className="games-list">
              {displayedGames.map((game, index) => (
                <div
                  key={index}
                  className="game-item"
                  onClick={() => game && handleGameClick(game.gameId)}
                  style={{ cursor: game ? "pointer" : "default" }}
                >
                  {game ? (
                    <>
                      <div style={{ cursor: "pointer" }}>
                        <ChessboardComponent
                          position={game.lastFen}
                          isDraggablePiece={() => false}
                          onSquareClick={() => false}
                          boardWidth={200}
                        />
                      </div>
                      <p
                        className={`game-result ${
                          game.result === username
                            ? "win"
                            : game.result === "Draw"
                            ? "draw"
                            : "loss"
                        }`}
                      >
                        {game.result === username
                          ? "You Won"
                          : game.result === "Draw"
                          ? "Draw"
                          : "You Lost"}
                      </p>
                    </>
                  ) : (
                    <div className="empty-board"></div>
                  )}
                </div>
              ))}
            </div>

            {!hasMore && !loading && (
              <div className="no-more-games">
                <p>That's it!</p>
              </div>
            )}
          </div>
          {loading && !initialLoading && (
            <div className="loading-spinner bounce-spinner">
              <div className="spinner"></div>
            </div>
          )}
        </div>
      )}
    </BackgroundUI>
  );
};

export default GameHistory;
