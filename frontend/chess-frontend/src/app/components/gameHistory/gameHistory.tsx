import React, { useState, useEffect, useRef } from "react";
import axios from "axios";
import BackgroundUI from "../backgroundUI/pages";
import { useSelector } from "react-redux";
import ChessboardComponent from "../chessBoard/chessBoard";
import "./gameHistory.css";
import { off } from "process";

const GameHistory = () => {
  const [games, setGames] = useState([]);
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [shouldFetchNext, setShouldFetchNext] = useState(false);
  const [offset, setOffset] = useState(0);
  const [hasMore, setHasMore] = useState(true);
  const { user } = useSelector((state) => state.user);
  const limit = 3;
  const listContainerRef = useRef(null); // Referencja do kontenera z listą
  var counter = 0; 
  useEffect(() => {
    if (user?.id && counter === 0) {
      fetchGames(user.id);
      counter++;
    }
  }, [user]);

  useEffect(() => {
    if (user?.id && shouldFetchNext) {
      fetchMoreGames(user.id);
    }
  }, [shouldFetchNext]);
  
  
  const fetchGames = async (playerId) => {
    try {
      setInitialLoading(true);
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
      const firstLimit = 6;
      const response = await axios.get(
        `${API_BASE_URL}/games/${playerId}?limit=${firstLimit}&offset=0&detailed=false`
      );

      if (response.data.length < firstLimit) {
        setHasMore(false); // Jeśli jest mniej niż `limit` gier, to koniec danych
      }
      setOffset((prevOffset) => prevOffset + firstLimit);
      setGames((prevGames) => [...prevGames, ...response.data]);
      setInitialLoading(false);
    } catch (error) {
      console.error("Error fetching game history:", error);
    } finally {
      setLoading(false);
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
        setHasMore(false); // Jeśli mniej niż `limit`, to koniec danych
      }
  
      if (response.data.length > 0) {
        setGames((prevGames) => [...prevGames, ...response.data]);
        setOffset((prevOffset) => prevOffset + limit); // Zwiększ offset tylko przy danych
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

  

  // Uzupełnij puste miejsca do 6 elementów
  const displayedGames = hasMore
  ? [...games, ...Array(limit - (games.length % limit)).fill(null)]
  : games;


  return (

    <BackgroundUI>
        <h1 style={{color: "white", fontSize: "18px", marginBottom:"5px" }}>Game history</h1>
    {initialLoading ? (
        <div className="loading-spinner initial-spinner">
          <div className="spinner"></div>
        </div>
      ) : (
      <div className="game-history">
      <div
        className={`games-list-container ${!hasMore ? "no-scroll" : ""}`}
         ref={listContainerRef}
        onScroll={handleScroll} // Obsługa przewijania
        >

        <div className="games-list">
        {displayedGames.map((game, index) => (
            <div key={index} className="game-item">
            {game ? (
                <>
                <ChessboardComponent
                    position={game.lastFen}
                    isDraggablePiece={() => false}
                    onSquareClick={() => false}
                    boardWidth={200}
                />
                <p
                    className={`game-result ${
                    game.result === user.username
                        ? "win"
                        : game.result === "Draw"
                        ? "draw"
                        : "loss"
                    }`}
                >
                    {game.result === user.username
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