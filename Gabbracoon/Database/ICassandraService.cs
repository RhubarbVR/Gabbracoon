using Cassandra;

namespace RhubarbServerNode.Database
{
	public interface ICassandraService
	{
        Cassandra.ISession DatabaseSession { get; }
	}
}
