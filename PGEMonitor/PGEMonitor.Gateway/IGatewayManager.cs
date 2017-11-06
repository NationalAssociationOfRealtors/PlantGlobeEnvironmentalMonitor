using System.Collections.Generic;
using System.Threading.Tasks;

namespace PGEMonitor.Gateway
{
    public interface IGatewayManager
    {
        Task InitAsync();
        IEnumerable<int> GetBoards();

        void SetDefaultBoard(int? id);

        int? GetSelectedBoard();

        bool TrySelectBoard(int id);

        Task<IEnumerable<BoardReading>> GetBoardReadingsAsync();
    }
}
